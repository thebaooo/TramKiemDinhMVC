using Microsoft.AspNetCore.Mvc;

namespace Test.Controllers
{
    public class ForbiddenController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
