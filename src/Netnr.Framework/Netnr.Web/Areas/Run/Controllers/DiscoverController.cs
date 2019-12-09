using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace Netnr.Web.Areas.Run.Controllers
{
    [Area("Run")]
    public class DiscoverController : Controller
    {
        /// <summary>
        /// Run列表
        /// </summary>
        /// <param name="q"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public IActionResult Index(string q, int page = 1)
        {
            var uinfo = new Func.UserAuthAid(HttpContext).Get();

            var ps = Func.Common.RunQuery(q, 0, uinfo.UserId, page);
            ps.Route = Request.Path;
            ViewData["q"] = q;
            return View("_PartialRunList", ps);
        }
    }
}