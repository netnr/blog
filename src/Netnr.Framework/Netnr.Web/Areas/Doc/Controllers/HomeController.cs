using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace Netnr.Web.Areas.Doc.Controllers
{
    [Area("Doc")]
    public class HomeController : Controller
    {
        /// <summary>
        /// Doc首页
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }
    }
}