using API.Services;
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
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Upload", "Sql2017", "ExcelFiles", $"{fileName}");

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound($"File with name {fileName} not found.");
                }

                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                using var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                Type? entityType = Type.GetType($"API.Data.Models.Entities.Sql2017.{tableName}");
                if (entityType == null)
                {
                    return BadRequest($"Entity type {tableName} not found.");
                }

                var (linesAdded, errorMessages) = await _excelImportService.ImportExcelData(memoryStream, entityType, dbName);

                return Ok(new { message = "File processed successfully.", linesAdded, errorMessages });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error during import: {ex.Message}");
            }
        }

        [HttpGet("Excel/Sql2017/deletedata")]
        public async Task<IActionResult> DeleteDataFromTable([FromQuery] string tableName)
        {
            try
            {
                var dbName = "msales-pro";

                // Resolve the entity type dynamically
                var entityType = Type.GetType($"API.Data.Models.Entities.Sql2017.{tableName}");
                if (entityType == null)
                {
                    return BadRequest($"Entity type '{tableName}' not found.");
                }

                // Get the generic method definition from the service
                var method = _excelImportService.GetType()
                    .GetMethod(nameof(_excelImportService.DeleteDataFromDatabase));

                // Make the method generic with the resolved entity type
                if (method == null)
                {
                    return StatusCode(500, "DeleteDataFromDatabase method not found.");
                }

                var genericMethod = method.MakeGenericMethod(entityType);

                if (genericMethod == null)
                {
                    return StatusCode(500, "Failed to create generic method.");
                }

                // Invoke the method dynamically, passing the database name
                var task = (Task?)genericMethod?.Invoke(_excelImportService, new object[] { dbName });
                if (task == null)
                {
                    return StatusCode(500, "Failed to invoke delete method.");
                }
                await task;

                return Ok(new { message = $"All data from '{tableName}' table has been deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting data: {ex.Message}");
            }
        }

    }
}