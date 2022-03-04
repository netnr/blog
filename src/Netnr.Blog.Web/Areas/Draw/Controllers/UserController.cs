using Netnr.Blog.Data;

namespace Netnr.Blog.Web.Areas.Draw.Controllers
{
    [Area("Draw")]
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
        /// <param name="page"></param>
        /// <returns></returns>
        public IActionResult Index(string q, int page = 1)
        {
            string id = RouteData.Values["id"]?.ToString();
            if (string.IsNullOrWhiteSpace(id))
            {
                return Redirect("/draw");
            }

            int uid = Convert.ToInt32(id);

            var mu = db.UserInfo.Find(uid);
            if (mu == null)
            {
                return Content("Account is empty");
            }
            ViewData["Nickname"] = mu.Nickname;

            var uinfo = Apps.LoginService.Get(HttpContext);

            var ps = Application.CommonService.DrawQuery(q, uid, uinfo.UserId, page);
            ps.Route = Request.Path;
            ViewData["q"] = q;
            return View("_PartialDrawList", ps);
        }
    }
}