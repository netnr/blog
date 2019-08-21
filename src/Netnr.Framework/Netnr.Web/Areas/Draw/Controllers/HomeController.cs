using Microsoft.AspNetCore.Mvc;

namespace Netnr.Web.Areas.Draw.Controllers
{
    [Area("Draw")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View("_PartialDrawView");
        }
    }
}