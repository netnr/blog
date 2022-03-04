using Netnr.Blog.Data;

namespace Netnr.Blog.Web.Areas.Run.Controllers
{
    [Area("Run")]
    public class CodeController : Controller
    {
        public ContextBase db;

        public CodeController(ContextBase cb)
        {
            db = cb;
        }

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
                var mo = db.Run.FirstOrDefault(x => x.RunCode == id && x.RunOpen == 1 && x.RunStatus == 1);
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
                        var mo = db.Run.FirstOrDefault(x => x.RunCode == id);
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
                            var uinfo = Apps.LoginService.Get(HttpContext);

                            var mo = db.Run.FirstOrDefault(x => x.RunCode == id && x.Uid == uinfo.UserId);
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

            var moout = db.Run.FirstOrDefault(x => x.RunCode == id && x.RunStatus == 1 && x.RunOpen == 1);
            ViewData["pure"] = pure;
            return View(moout);
        }
    }
}