using Dynamsoft.CVR;
using Dynamsoft.DBR;
using Dynamsoft.Core;
using Microsoft.AspNetCore.Mvc;
using OpenCvSharp;

namespace DynamsoftRestfulService.Controllers
{
    [Route("dynamsoft/[controller]")]
    [ApiController]
    public class DbrController : ControllerBase
    {
        private CaptureVisionRouter cvRouter;

        public DbrController()
        {
            cvRouter = new CaptureVisionRouter();
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
                    byte[] imageBytes = memoryStream.ToArray();

                    CapturedResult capturedResult = cvRouter.Capture(imageBytes, PresetTemplate.PT_READ_BARCODES);

                    if (capturedResult.GetErrorCode() != (int)EnumErrorCode.EC_OK &&
                        capturedResult.GetErrorCode() != (int)EnumErrorCode.EC_UNSUPPORTED_JSON_KEY_WARNING)
                    {
                        return Ok($"Error: {capturedResult.GetErrorString()}");
                    }

                    DecodedBarcodesResult? barcodesResult = capturedResult.GetDecodedBarcodesResult();
                    BarcodeResultItem[]? items = barcodesResult?.GetItems();

                    if (items == null || items.Length == 0)
                    {
                        return Ok("No barcode found");
                    }

                    var results = items.Select(item => new
                    {
                        text = item.GetText(),
                        format = item.GetFormatString()
                    }).ToArray();

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
