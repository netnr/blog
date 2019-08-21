using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace Netnr.Web.Areas.Doc.Controllers
{
    [Area("Doc")]
    public class HomeController : Controller
    {
        [Description("Doc首页")]
        public IActionResult Index()
        {
            return View();
        }
    }
}