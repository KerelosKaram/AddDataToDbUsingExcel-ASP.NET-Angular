using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class DbAxController : BaseApiController
    {
        private readonly ExcelImportService _excelImportService;
        public DbAxController(ExcelImportService excelImportService)
        {
            _excelImportService = excelImportService;
        }

        [HttpPost("Excel/DbAx/upload")]
        public async Task<ActionResult> UploadExcelFileAsync([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var filepath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Upload", "DBAX", "ExcelFiles", file.FileName);

            Directory.CreateDirectory(Path.GetDirectoryName(filepath) ?? "wwwroot/Upload/DbAx/ExcelFiles");

            await using var fileStream = new FileStream(filepath, FileMode.Create);

            await file.CopyToAsync(fileStream);

            return Ok(new { filePath = filepath});
        }

        [HttpPost("Excel/DbAx/insertdata")]
        public async Task<IActionResult> InsertDataFromExcelFile([FromQuery] string fileName, [FromQuery] string tableName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name is required.");
            }

            try
            {
                var dbName = "Alamir_AX6_Live";

                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Upload", "DBAX", "ExcelFiles", $"{fileName}");

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound($"File with name {fileName} not found.");
                }

                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                using var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                Type? entityType = Type.GetType($"API.Data.Models.Entities.DBAX.{tableName}");
                if (entityType == null)
                {
                    return BadRequest($"Entity type {tableName} not found.");
                }

                var (linesAdded, errorMessages) = await _excelImportService.ImportExcelData(memoryStream, entityType, dbName);

                return Ok(new { message = "File processed successfully.", linesAdded, errorMessages });

            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Error during import: {ex.Message}");
            }
        }

        [HttpGet("Excel/DbAx/deletedata")]
        public async Task<ActionResult> DeleteDataFromTable([FromQuery] string tableName)
        {
            try
            {
                var dbName = "Alamir_AX6_Live";

                var entityType = Type.GetType($"API.Data.Models.Entities.DBAX.{tableName}");
                // Console.WriteLine("We are here:" + entityType);
                if (entityType == null)
                {
                    return BadRequest($"Entity type '{tableName}' not found.");
                }

                var method = _excelImportService.GetType()
                    .GetMethod(nameof(_excelImportService.DeleteDataFromDatabase));

                if (method == null)
                {
                    return StatusCode(500, "DeleteDataFromDatabase method not found.");
                }

                var genericMethod = method.MakeGenericMethod(entityType);

                if (genericMethod == null)
                {
                    return StatusCode(500, "Failed to create generic method.");
                }

                var task = (Task?)genericMethod?.Invoke(_excelImportService, [dbName]);
                if (task == null)
                {
                    return StatusCode(500, "Failed to invoke delete method.");
                }

                await task;

                return Ok(new { message = $"All data from '{tableName}' table has been deleted successfully."});
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                Console.WriteLine(ex.Message);
                return StatusCode(500, $"Error deleting data: {ex.Message}");
            }
        }
    }
}