using Microsoft.AspNetCore.Mvc;

namespace Netnr.Web.Areas.Draw.Controllers
{
    [Area("Draw")]
    public class MindController : Controller
    {
        /// <summary>
        /// 脑图首页
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View("_PartialMindView");
        }
    }
}