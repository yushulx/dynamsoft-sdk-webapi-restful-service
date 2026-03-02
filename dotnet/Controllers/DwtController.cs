using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Twain.Wia.Sane.Scanner;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        public async Task<ActionResult> GetDevices()
        {
            // v2.x: GetDevices returns a JSON string instead of a List<>
            string scannerJson = await scannerController.GetDevices(host);
            var scanners = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(scannerJson);
            if (scanners == null || scanners.Count == 0)
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

                    // Use Newtonsoft.Json to handle nested dictionaries correctly
                    var parameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    if (parameters == null) return BadRequest("No content found");

                    // Convert JObjects/JArrays to native Dictionary/List to ensure compatibility with the SDK
                    parameters = (Dictionary<string, object>)ToNative(JObject.FromObject(parameters));

                    parameters["license"] = LicenseManager.licenseKey;
                    parameters["autoRun"] = true;

                    // v2.x: CreateJob replaces ScanDocument; returns a JSON string with "jobuid"
                    string jobJson = await scannerController.CreateJob(host, parameters);

                    var jobObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(jobJson);
                    
                    if (jobObj == null || !jobObj.ContainsKey("jobuid"))
                    {
                        return BadRequest("Failed to create scan job: " + jobJson);
                    }

                    string jobId = jobObj["jobuid"]?.ToString() ?? "";
                    return Ok(jobId);
                }
            }
            else
            {
                return BadRequest("Unsupported content type");
            }
        }

        private object ToNative(JToken token)
        {
            if (token is JObject obj)
            {
                var dict = new Dictionary<string, object>();
                foreach (var prop in obj)
                {
                    dict[prop.Key] = ToNative(prop.Value);
                }
                return dict;
            }
            else if (token is JArray arr)
            {
                var list = new List<object>();
                foreach (var item in arr)
                {
                    list.Add(ToNative(item));
                }
                return list;
            }
            else if (token is JValue val)
            {
                return val.Value;
            }
            return token.ToString();
        }


        // GET dynamsoft/dwt/GetImageStream/jobId
        [HttpGet]
        [Route("GetImageStream/{jobId}")]
        public async Task<IActionResult> GetImageStream(string jobId)
        {
            byte[] bytes = await scannerController.GetImageStream(host, jobId);
            if (bytes == null || bytes.Length == 0)
            {
                // If no more images, try to delete the job to clean up
                await scannerController.DeleteJob(host, jobId);
                return Ok("No image found");
            }
            else
            {
                return File(bytes, "application/octet-stream");
            }
        }
    }
}
