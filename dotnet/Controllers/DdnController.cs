using Dynamsoft.CVR;
using Dynamsoft.DDN;
using Dynamsoft.Core;
using Microsoft.AspNetCore.Mvc;
using OpenCvSharp;

namespace DynamsoftRestfulService.Controllers
{
    [Route("dynamsoft/[controller]")]
    [ApiController]
    public class DdnController : ControllerBase
    {
        private CaptureVisionRouter cvRouter;

        public DdnController()
        {
            cvRouter = new CaptureVisionRouter();
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
                    byte[] imageBytes = memoryStream.ToArray();

                    CapturedResult capturedResult = cvRouter.Capture(imageBytes, PresetTemplate.PT_DETECT_AND_NORMALIZE_DOCUMENT);

                    if (capturedResult.GetErrorCode() != (int)EnumErrorCode.EC_OK &&
                        capturedResult.GetErrorCode() != (int)EnumErrorCode.EC_UNSUPPORTED_JSON_KEY_WARNING)
                    {
                        return Ok($"Error: {capturedResult.GetErrorString()}");
                    }

                    ProcessedDocumentResult? docResult = capturedResult.GetProcessedDocumentResult();
                    EnhancedImageResultItem[]? enhancedItems = docResult?.GetEnhancedImageResultItems();

                    if (enhancedItems != null && enhancedItems.Length > 0)
                    {
                        ImageData? imageData = enhancedItems[0].GetImageData();
                        if (imageData != null)
                        {
                            byte[] rawBytes = imageData.GetBytes();
                            int width = imageData.GetWidth();
                            int height = imageData.GetHeight();
                            int stride = imageData.GetStride();
                            EnumImagePixelFormat format = imageData.GetImagePixelFormat();

                            Mat newMat;
                            if (format == EnumImagePixelFormat.IPF_GRAYSCALED)
                            {
                                newMat = new Mat(height, width, MatType.CV_8UC1, rawBytes);
                            }
                            else if (format == EnumImagePixelFormat.IPF_BINARY || stride < width)
                            {
                                // binary – expand to grayscale
                                newMat = new Mat(height, width, MatType.CV_8UC1, rawBytes);
                            }
                            else
                            {
                                newMat = new Mat(height, width, MatType.CV_8UC3, rawBytes);
                            }

                            Cv2.ImEncode(".jpg", newMat, out byte[] buffer);
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
