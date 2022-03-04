using Netnr.Blog.Data;

namespace Netnr.Blog.Web.Areas.Gist.Controllers
{
    [Area("Gist")]
    public class UserController : Controller
    {
        public ContextBase db;

        public UserController(ContextBase cb)
        {
            db = cb;
        }

        /// <summary>
        /// 用户
        /// </summary>
        /// <param name="q"></param>
        /// <param name="lang"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public IActionResult Index(string q, string lang, int page = 1)
        {
            string id = RouteData.Values["id"]?.ToString();
            if (string.IsNullOrWhiteSpace(id))
            {
                return Redirect("/gist");
            }

            int uid = Convert.ToInt32(id);

            var mu = db.UserInfo.Find(uid);
            if (mu == null)
            {
                return Content("Account is empty");
            }
            ViewData["Nickname"] = mu.Nickname;

            var uinfo = Apps.LoginService.Get(HttpContext);

            var ps = Application.CommonService.GistQuery(q, lang, uid, uinfo.UserId, page);
            ps.Route = Request.Path;
            ViewData["lang"] = lang;
            ViewData["q"] = q;
            return View("_PartialGistList", ps);
        }
    }
}