
using Microsoft.AspNetCore.Mvc;
using webapp.Models;

namespace webapp.Controllers
{
    public class TestController : Controller
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View("Test", new TestModel());
        }

        public IActionResult GetValue()
        {
            return View("Test", new TestModel { Message = "Clicked test !" });
        }

        public IActionResult GetValuePartial()
        {
            return PartialView("_TestPartial", new TestModel { Message = "Clicked test !" });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}