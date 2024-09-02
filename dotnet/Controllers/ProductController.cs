using Microsoft.AspNetCore.Mvc;

namespace DynamsoftRestfulService.Controllers
{
    [Route("dynamsoft/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private static List<String> products = new List<String>
        {
            "Dynamic Web TWAIN",
            "Dynamsoft Barcode Reader",
            "Dynamsoft Label Recognizer",
            "Dynamsoft Document Normalizer",
        };

        // GET dynamsoft/product
        [HttpGet]
        public ActionResult<IEnumerable<String>> Get()
        {
            return products;
        }

    }
}
