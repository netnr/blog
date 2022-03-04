namespace Netnr.Blog.Web.Areas.Run.Controllers
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
            var uinfo = Apps.LoginService.Get(HttpContext);

            var ps = Application.CommonService.RunQuery(q, 0, uinfo.UserId, page);
            ps.Route = Request.Path;
            ViewData["q"] = q;
            return View("_PartialRunList", ps);
        }
    }
}