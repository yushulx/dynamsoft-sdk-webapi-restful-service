using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Dynamsoft;
using static Dynamsoft.MrzScanner;
using Result = Dynamsoft.MrzScanner.Result;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Runtime.InteropServices;

namespace DynamsoftRestfulService.Controllers
{
    [Route("dynamsoft/[controller]")]
    [ApiController]
    public class DlrController : ControllerBase
    {
        private MrzScanner mrzScanner;

        public DlrController()
        {
            mrzScanner = MrzScanner.Create();
            mrzScanner.LoadModel();
        }

        // POST dynamsoft/dlr/DetectMrz
        [HttpPost]
        [Route("DetectMrz")]
        public async Task<IActionResult> DetectMrz()
        {
            if (Request.ContentType == null) return BadRequest("No content type specified");

            if (Request.ContentType.Contains("multipart/form-data"))
            {
                var form = await Request.ReadFormAsync();
                var file = form.Files[0];
                if (file.Length == 0)
                {
                    return BadRequest("Empty file received");
                }

                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;
                    Mat mat = Mat.FromStream(memoryStream, ImreadModes.Color);

                    int length = mat.Cols * mat.Rows * mat.ElemSize();
                    byte[] bytes = new byte[length];
                    Marshal.Copy(mat.Data, bytes, 0, length);
                    Result[]? results = mrzScanner.DetectBuffer(bytes, mat.Cols, mat.Rows, (int)mat.Step(), MrzScanner.ImagePixelFormat.IPF_RGB_888);
                    if (results == null || results.Length == 0)
                    {
                        return Ok("No MRZ found");
                    }

                    string[] lines = new string[results.Length];
                    var index = 0;
                    foreach (Result result in results)
                    {
                        lines[index++] = result.Text;
                    }

                    MrzResult info = MrzParser.Parse(lines);

                    return Ok(info.ToJson());
                }
            }
            else
            {
                return BadRequest("Unsupported content type");
            }
        }
    }
}
