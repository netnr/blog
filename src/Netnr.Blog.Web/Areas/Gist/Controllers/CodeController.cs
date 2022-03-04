using Netnr.Blog.Data;

namespace Netnr.Blog.Web.Gist.Controllers
{
    [Area("Gist")]
    public class CodeController : Controller
    {
        public ContextBase db;

        public CodeController(ContextBase cb)
        {
            db = cb;
        }

        /// <summary>
        /// Gist一条操作
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            string id = RouteData.Values["id"]?.ToString();
            if (string.IsNullOrWhiteSpace(id))
            {
                return Redirect("/gist");
            }

            //cmd && Auth
            string cmd = RouteData.Values["sid"]?.ToString().ToLower();
            if (User.Identity.IsAuthenticated)
            {
                var uinfo = Apps.LoginService.Get(HttpContext);
                switch (cmd)
                {
                    case "edit":
                        {
                            var mo = db.Gist.Where(x => x.GistCode == id).FirstOrDefault();
                            //有记录且为当前用户
                            if (mo != null && mo.Uid == uinfo.UserId)
                            {
                                return View("_PartialMonacoEditor", mo);
                            }
                        }
                        break;
                    case "delete":
                        {
                            var mo = db.Gist.Where(x => x.GistCode == id && x.Uid == uinfo.UserId).FirstOrDefault();
                            db.Gist.Remove(mo);
                            int num = db.SaveChanges();
                            if (num > 0)
                            {
                                return Redirect("/gist/user/" + uinfo.UserId);
                            }
                            else
                            {
                                return Content("Deletion failed or could not be accessed");
                            }
                        }
                }
            }

            var query = from a in db.Gist
                        join b in db.GistSync on a.GistCode equals b.GistCode into bg
                        from b in bg.DefaultIfEmpty()
                        join c in db.UserInfo on a.Uid equals c.UserId
                        where a.GistCode == id && a.GistStatus == 1 && a.GistOpen == 1
                        select new Domain.Gist
                        {
                            GistId = a.GistId,
                            Uid = a.Uid,
                            GistCode = a.GistCode,
                            GistContent = a.GistContent,
                            GistCreateTime = a.GistCreateTime,
                            GistUpdateTime = a.GistUpdateTime,
                            GistFilename = a.GistFilename,
                            GistLanguage = a.GistLanguage,
                            GistOpen = a.GistOpen,
                            GistRemark = a.GistRemark,
                            GistRow = a.GistRow,
                            GistStatus = a.GistStatus,
                            GistTags = a.GistTags,
                            GistTheme = a.GistTheme,

                            Spare1 = b == null ? null : b.GsGitHubId,
                            Spare2 = b == null ? null : b.GsGiteeId,
                            Spare3 = c.Nickname
                        };
            var moout = query.FirstOrDefault();

            return View(moout);
        }
    }
}