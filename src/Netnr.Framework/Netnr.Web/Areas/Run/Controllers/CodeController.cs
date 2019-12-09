using System.ComponentModel;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Netnr.Data;

namespace Netnr.Web.Areas.Run.Controllers
{
    [Area("Run")]
    public class CodeController : Controller
    {
        /// <summary>
        /// Run一条
        /// </summary>
        /// <param name="pure"></param>
        /// <returns></returns>
        public IActionResult Index(string pure)
        {
            string id = RouteData.Values["id"]?.ToString();
            if (string.IsNullOrWhiteSpace(id))
            {
                return Redirect("/run");
            }

            //output json
            if (!string.IsNullOrWhiteSpace(id) && id.ToLower().Contains(".json"))
            {
                id = id.Replace(".json", "");
                using var db = new ContextBase();
                var mo = db.Run.Where(x => x.RunCode == id && x.RunOpen == 1 && x.RunStatus == 1).FirstOrDefault();
                if (mo != null)
                {
                    return Content(new
                    {
                        code = mo.RunCode,
                        remark = mo.RunRemark,
                        datetime = mo.RunCreateTime,
                        html = mo.RunContent1,
                        javascript = mo.RunContent2,
                        css = mo.RunContent3
                    }.ToJson());
                }
            }

            //cmd (Auth)
            string cmd = RouteData.Values["sid"]?.ToString().ToLower();
            switch (cmd)
            {
                case "edit":
                    {
                        using var db = new ContextBase();
                        var mo = db.Run.Where(x => x.RunCode == id).FirstOrDefault();
                        //有记录且为当前用户
                        if (mo != null)
                        {
                            return View("_PartialMonacoEditor", mo);
                        }
                    }
                    break;
                case "delete":
                    {
                        if (User.Identity.IsAuthenticated)
                        {
                            using var db = new ContextBase();
                            var uinfo = new Func.UserAuthAid(HttpContext).Get();
                            var mo = db.Run.Where(x => x.RunCode == id && x.Uid == uinfo.UserId).FirstOrDefault();
                            db.Run.Remove(mo);
                            int num = db.SaveChanges();
                            if (num > 0)
                            {
                                return Redirect("/run/user/" + uinfo.UserId);
                            }
                            else
                            {
                                return Content("Deletion failed or could not be accessed");
                            }
                        }
                        else
                        {
                            return Content("Deletion failed or could not be accessed");
                        }
                    }
            }

            using (var db = new ContextBase())
            {
                var mo = db.Run.Where(x => x.RunCode == id && x.RunStatus == 1 && x.RunOpen == 1).FirstOrDefault();
                ViewData["pure"] = pure;
                return View(mo);
            }
        }
    }
}