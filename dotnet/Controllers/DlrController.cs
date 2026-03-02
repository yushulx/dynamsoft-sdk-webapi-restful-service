using Dynamsoft.CVR;
using Dynamsoft.DLR;
using Dynamsoft.DCP;
using Dynamsoft.Core;
using Microsoft.AspNetCore.Mvc;

namespace DynamsoftRestfulService.Controllers
{
    [Route("dynamsoft/[controller]")]
    [ApiController]
    public class DlrController : ControllerBase
    {
        private CaptureVisionRouter cvRouter;

        public DlrController()
        {
            cvRouter = new CaptureVisionRouter();
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
                    byte[] imageBytes = memoryStream.ToArray();

                    CapturedResult capturedResult = cvRouter.Capture(imageBytes, "ReadPassportAndId");

                    if (capturedResult.GetErrorCode() != (int)EnumErrorCode.EC_OK &&
                        capturedResult.GetErrorCode() != (int)EnumErrorCode.EC_UNSUPPORTED_JSON_KEY_WARNING)
                    {
                        return Ok($"Error: {capturedResult.GetErrorString()}");
                    }

                    ParsedResult? parsedResult = capturedResult.GetParsedResult();
                    ParsedResultItem[]? items = parsedResult?.GetItems();

                    if (items == null || items.Length == 0)
                    {
                        return Ok("No MRZ found");
                    }

                    var mrzResults = items.Select(item =>
                    {
                        string docType = item.GetCodeType();
                        string? docId = docType == "MRTD_TD3_PASSPORT" ?
                            item.GetFieldValue("passportNumber") : item.GetFieldValue("documentNumber");

                        return new
                        {
                            documentType = docType,
                            documentId = docId,
                            surname = item.GetFieldValue("primaryIdentifier"),
                            givenName = item.GetFieldValue("secondaryIdentifier"),
                            nationality = item.GetFieldValue("nationality"),
                            issuingCountry = item.GetFieldValue("issuingState"),
                            dateOfBirth = item.GetFieldValue("dateOfBirth"),
                            dateOfExpiry = item.GetFieldValue("dateOfExpiry"),
                            sex = item.GetFieldValue("sex"),
                            line1 = item.GetFieldValue("line1"),
                            line2 = item.GetFieldValue("line2"),
                            line3 = item.GetFieldValue("line3")
                        };
                    }).ToArray();

                    return Ok(mrzResults);
                }
            }
            else
            {
                return BadRequest("Unsupported content type");
            }
        }
    }
}
