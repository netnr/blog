using Microsoft.AspNetCore.Mvc;

namespace Netnr.Web.Areas.Draw.Controllers
{
    [Area("Draw")]
    public class MindController : Controller
    {
        public IActionResult Index()
        {
            return View("_PartialMindView");
        }
    }
}