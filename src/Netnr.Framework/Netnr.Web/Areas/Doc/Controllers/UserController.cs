using System;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using Netnr.Data;

namespace Netnr.Web.Areas.Doc.Controllers
{
    [Area("Doc")]
    public class UserController : Controller
    {
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
                return Redirect("/doc");
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

            var ps = Func.Common.DocQuery(q, uid, uinfo.UserId, page);
            ps.Route = Request.Path;
            ViewData["q"] = q;
            return View("_PartialDocList", ps);
        }
    }
}