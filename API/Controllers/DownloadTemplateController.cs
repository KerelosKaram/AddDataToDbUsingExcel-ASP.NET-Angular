using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class DownloadTemplateController : BaseApiController
    {

        [HttpGet("template/download")]
        public async Task<IActionResult> DownloadExcelFileAsync([FromQuery] string templateName)
        {
            string path = Path.Combine("wwwroot", "Templates", $"{templateName}_API.xlsx");

            if (System.IO.File.Exists(path))  
            {
                var fileStream = await Task.Run(() => System.IO.File.OpenRead(path));
                return File(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Path.GetFileName(path));  
            }  
            return NotFound(); 
        }
    }
}