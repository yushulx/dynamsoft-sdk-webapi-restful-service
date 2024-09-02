using Dynamsoft;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using static Dynamsoft.BarcodeQRCodeReader;
using BarcodeResult = Dynamsoft.BarcodeQRCodeReader.Result;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Runtime.InteropServices;

namespace DynamsoftRestfulService.Controllers
{
    [Route("dynamsoft/[controller]")]
    [ApiController]
    public class DbrController : ControllerBase
    {
        private BarcodeQRCodeReader barcodeScanner;

        public DbrController()
        {
            barcodeScanner = BarcodeQRCodeReader.Create();
        }

        // POST dynamsoft/dbr/DecodeBarcode
        [HttpPost]
        [Route("DecodeBarcode")]
        public async Task<IActionResult> DecodeBarcode()
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
                    BarcodeResult[]? results = barcodeScanner.DecodeBuffer(bytes, mat.Cols, mat.Rows, (int)mat.Step(), BarcodeQRCodeReader.ImagePixelFormat.IPF_RGB_888);
                    if (results == null || results.Length == 0)
                    {
                        return Ok("No barcode found");
                    }

                    return Ok(results);
                }
            }
            else
            {
                return BadRequest("Unsupported content type");
            }
        }
    }
}
