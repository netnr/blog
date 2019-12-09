using System;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using Netnr.Data;

namespace Netnr.Web.Areas.Gist.Controllers
{
    [Area("Gist")]
    public class UserController : Controller
    {
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

            using (var db = new ContextBase())
            {
                var mu = db.UserInfo.Find(uid);
                if (mu == null)
                {
                    return Content("Account is empty");
                }
                ViewData["Nickname"] = mu.Nickname;
            }

            var uinfo = new Func.UserAuthAid(HttpContext).Get();

            var ps = Func.Common.GistQuery(q, lang, uid, uinfo.UserId, page);
            ps.Route = Request.Path;
            ViewData["lang"] = lang;
            ViewData["q"] = q;
            return View("_PartialGistList", ps);
        }
    }
}