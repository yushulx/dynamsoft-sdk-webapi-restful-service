using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Dynamsoft;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using static Dynamsoft.DocumentScanner;
using DocResult = Dynamsoft.DocumentScanner.Result;
using System.Runtime.InteropServices;

namespace DynamsoftRestfulService.Controllers
{
    [Route("dynamsoft/[controller]")]
    [ApiController]
    public class DdnController : ControllerBase
    {
        private DocumentScanner documentScanner;

        public DdnController()
        {
            documentScanner = DocumentScanner.Create();
            documentScanner.SetParameters(DocumentScanner.Templates.color);
        }

        // POST dynamsoft/ddn/rectifyDocument
        [HttpPost]
        [Route("rectifyDocument")]
        public async Task<IActionResult> rectifyDocument()
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
                    DocResult[]? results = documentScanner.DetectBuffer(bytes, mat.Cols, mat.Rows, (int)mat.Step(), DocumentScanner.ImagePixelFormat.IPF_RGB_888);

                    if (results != null)
                    {
                        DocResult result = results[0];

                        NormalizedImage image = documentScanner.NormalizeBuffer(bytes, mat.Cols, mat.Rows, (int)mat.Step(), DocumentScanner.ImagePixelFormat.IPF_RGB_888, result.Points);
                        if (image != null && image.Data != null)
                        {
                            Mat newMat;
                            if (image.Stride < image.Width)
                            {
                                // binary
                                byte[] data = image.Binary2Grayscale();
                                newMat = new Mat(image.Height, image.Width, MatType.CV_8UC1, data);
                            }
                            else if (image.Stride >= image.Width * 3)
                            {
                                // color
                                newMat = new Mat(image.Height, image.Width, MatType.CV_8UC3, image.Data);
                            }
                            else
                            {
                                // grayscale
                                newMat = new Mat(image.Height, image.Stride, MatType.CV_8UC1, image.Data);
                            }

                            byte[] buffer;
                            Cv2.ImEncode(".jpg", newMat, out buffer);
                            return File(buffer, "application/octet-stream");
                        }

                    }
                }

                return Ok("No document found");
            }
            else
            {
                return BadRequest("Unsupported content type");
            }
        }
    }
}
