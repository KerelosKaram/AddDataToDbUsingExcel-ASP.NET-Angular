using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    // [Authorize(Policy = "HasSmsClaim")]  // Apply the policy to the entire controller
    public class OneNineTwoController : BaseApiController
    {
        private readonly ExcelImportService _excelImportService;

        public OneNineTwoController(ExcelImportService excelImportService)
        {
            _excelImportService = excelImportService;
        }

        [HttpPost("Excel/OneNineTwo/upload")]
        public async Task<ActionResult> UploadExcelFileAsync([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Save the file to a temporary location
            var filepath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Upload", "OneNineTwo", "ExcelFiles", file.FileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filepath) ?? "wwwroot/Upload/OneNineTwo/ExcelFiles");

            await using var fileStream = new FileStream(filepath, FileMode.Create);
            await file.CopyToAsync(fileStream);

            // Return the file path or an identifier (e.g., filename)
            return Ok(new { filePath = filepath });
        }

        [HttpPost("Excel/OneNineTwo/insertdata")]
        public async Task<IActionResult> InsertDataFromExcelFile([FromQuery] string fileName, [FromQuery] string tableName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name is required.");
            }
            
            try
            {
                var dbName = "SMSServer";
                // Construct the file path using the provided file name
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Upload", "OneNineTwo", "ExcelFiles", $"{fileName}");

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
                Type entityType = Type.GetType($"API.Data.Models.Entities.OneNineTwo.{tableName}");
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


        // [HttpGet("Excel/OneNineTwo/sms/download")]
        // public async Task<IActionResult> DownloadExcelFileAsync()
        // {
        //     string path = Path.Combine("wwwroot", "Templates", "SMS_API.xlsx");

        //     if (System.IO.File.Exists(path))  
        //     {
        //         var fileStream = await Task.Run(() => System.IO.File.OpenRead(path));
        //         return File(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Path.GetFileName(path));  
        //     }  
        //     return NotFound(); 
        // }
    }
}