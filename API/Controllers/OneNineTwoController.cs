using API.Services;
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
                Type? entityType = Type.GetType($"API.Data.Models.Entities.OneNineTwo.{tableName}");
                if (entityType == null)
                {
                    return BadRequest($"Entity type {tableName} not found.");
                }

                // Pass the MemoryStream and entity type to the service for processing
                // await _excelImportService.ImportExcelData(memoryStream, entityType, dbName);
                var (linesAdded, errorMessages) = await _excelImportService.ImportExcelData(memoryStream, entityType, dbName);

                return Ok(new { message = "File processed successfully.", linesAdded, errorMessages });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error during import: {ex.Message}");
            }
        }


        // Still Not Used (For future if needed)
        [HttpGet("Excel/OneNineTwo/deletedata")]
        public async Task<IActionResult> DeleteDataFromTable([FromQuery] string tableName)
        {
            try
            {
                var dbName = "SMSServer";

                // Resolve the entity type dynamically
                var entityType = Type.GetType($"API.Data.Models.Entities.OneNineTwo.{tableName}");
                if (entityType == null)
                {
                    return BadRequest($"Entity type '{tableName}' not found.");
                }

                // Get the generic method definition from the service
                var method = _excelImportService.GetType()
                    .GetMethod(nameof(_excelImportService.DeleteDataFromDatabase));

                if (method == null)
                {
                    return StatusCode(500, "DeleteDataFromDatabase method not found.");
                }

                // Make the method generic with the resolved entity type
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