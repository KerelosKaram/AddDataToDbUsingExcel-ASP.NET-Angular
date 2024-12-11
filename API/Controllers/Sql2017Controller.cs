using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class Sql2017Controller : BaseApiController
    {
        private readonly ExcelImportService _excelImportService;

        public Sql2017Controller(ExcelImportService excelImportService)
        {
            _excelImportService = excelImportService;
        }

        [HttpPost("Excel/Sql2017/upload")]
        public async Task<ActionResult> UploadExcelFileAsync([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Save the file to a temporary location
            var filepath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Upload", "Sql2017", "ExcelFiles", file.FileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filepath) ?? "wwwroot/Upload/Sql2017/ExcelFiles");

            await using var fileStream = new FileStream(filepath, FileMode.Create);
            await file.CopyToAsync(fileStream);

            // Return the file path or an identifier (e.g., filename)
            return Ok(new { filePath = filepath });
        }

        [HttpPost("Excel/Sql2017/insertdata")]
        public async Task<IActionResult> InsertDataFromExcelFile([FromQuery] string fileName, [FromQuery] string tableName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name is required.");
            }

            try
            {
                var dbName = "msales-pro";
                // Construct the file path using the provided file name
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Upload", "Sql2017", "ExcelFiles", $"{fileName}");

                // Check if the file exists
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound($"File with name {fileName} not found.");
                }

                // Create a MemoryStream to store the file content
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                using var memoryStream = new MemoryStream();
                
                // Copy the content of the FileStream to the MemoryStream
                await fileStream.CopyToAsync(memoryStream);

                // Reset the position of the memory stream before using it
                memoryStream.Position = 0;

                // Dynamically get the entity type from the provided name
                Type entityType = Type.GetType($"API.Data.Models.Entities.Sql2017.{tableName}");
                if (entityType == null)
                {
                    return BadRequest($"Entity type {tableName} not found.");
                }

                // Pass the MemoryStream and entity type to the service for processing
                await _excelImportService.ImportExcelData(memoryStream, entityType, dbName);

                return Ok(new { message = "File processed successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error during import: {ex.Message}");
            }
        }
    }
}