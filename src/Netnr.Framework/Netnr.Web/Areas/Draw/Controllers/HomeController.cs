using Microsoft.AspNetCore.Mvc;

namespace Netnr.Web.Areas.Draw.Controllers
{
    [Area("Draw")]
    public class HomeController : Controller
    {
        /// <summary>
        /// 绘制首页
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View("_PartialDrawView");
        }
    }
}