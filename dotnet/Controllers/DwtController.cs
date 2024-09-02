using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Twain.Wia.Sane.Scanner;
using System.Text.Json;

namespace DynamsoftRestfulService.Controllers
{
    [Route("dynamsoft/[controller]")]
    [ApiController]
    public class DwtController : ControllerBase
    {
        private static ScannerController scannerController = new ScannerController();
        private static string host = "http://127.0.0.1:18622";

        // GET dynamsoft/dwt/GetDevices
        [HttpGet]
        [Route("GetDevices")]
        public async Task<ActionResult<Dictionary<string, object>>> GetDevices()
        {
            var scanners = await scannerController.GetDevices(host);
            if (scanners.Count == 0)
            {
                return Ok("No devices found");
            }
            return Ok(scanners);
        }

        // POST dynamsoft/dwt/ScanDocument
        [HttpPost]
        [Route("ScanDocument")]
        public async Task<IActionResult> ScanDocument()
        {
            if (Request.ContentType == null) return BadRequest("No content type specified");

            if (Request.ContentType.Contains("application/json"))
            {
                using (StreamReader reader = new StreamReader(Request.Body))
                {
                    var json = await reader.ReadToEndAsync();

                    Dictionary<string, object>? parameters = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                    if (parameters == null) return BadRequest("No content found");

                    parameters["license"] = LicenseManager.dwt;
                    string jobId = await scannerController.ScanDocument(host, parameters);

                    return Ok(jobId);
                }
            }
            else
            {
                return BadRequest("Unsupported content type");
            }
        }

        // GET dynamsoft/dwt/GetImageStream/jobId
        [HttpGet]
        [Route("GetImageStream/{jobId}")]
        public async Task<ActionResult<Dictionary<string, object>>> GetImageStream(string jobId)
        {
            byte[] bytes = await scannerController.GetImageStream(host, jobId);
            if (bytes.Length == 0)
            {
                return Ok("No image found");
            }
            else
            {
                return File(bytes, "application/octet-stream");
            }
        }
    }
}
