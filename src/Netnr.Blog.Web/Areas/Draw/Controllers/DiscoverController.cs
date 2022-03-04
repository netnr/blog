namespace Netnr.Blog.Web.Areas.Draw.Controllers
{
    [Area("Draw")]
    public class DiscoverController : Controller
    {
        /// <summary>
        /// Draw列表
        /// </summary>
        /// <param name="q"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public IActionResult Index(string q, int page = 1)
        {
            var uinfo = Apps.LoginService.Get(HttpContext);

            var ps = Application.CommonService.DrawQuery(q, 0, uinfo.UserId, page);
            ps.Route = Request.Path;
            ViewData["q"] = q;
            return View("_PartialDrawList", ps);
        }
    }
}