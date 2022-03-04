using Microsoft.AspNetCore.Authorization;
using Netnr.Blog.Data;

namespace Netnr.Blog.Web.Areas.Gist.Controllers
{
    [Area("Gist")]
    public class HomeController : Controller
    {
        public ContextBase db;

        public HomeController(ContextBase cb)
        {
            db = cb;
        }

        /// <summary>
        /// Gist首页
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View("_PartialMonacoEditor");
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="mo"></param>
        /// <returns></returns>
        [Authorize]
        public SharedResultVM SaveGist(Domain.Gist mo)
        {
            var vm = Apps.LoginService.CompleteInfoValid(HttpContext);
            if (vm.Code == 200)
            {
                var uinfo = Apps.LoginService.Get(HttpContext);

                //add
                if (string.IsNullOrWhiteSpace(mo.GistCode))
                {
                    mo.GistId = Guid.NewGuid().ToString();
                    mo.GistCreateTime = DateTime.Now;
                    mo.GistUpdateTime = mo.GistCreateTime;
                    mo.GistStatus = 1;
                    mo.Uid = uinfo.UserId;

                    mo.GistCode = Core.UniqueTo.LongId().ToString();
                    db.Gist.Add(mo);
                    db.SaveChanges();

                    vm.Data = mo.GistCode;
                    vm.Set(SharedEnum.RTag.success);
                }
                else
                {
                    var oldmo = db.Gist.FirstOrDefault(x => x.GistCode == mo.GistCode);
                    if (oldmo?.Uid == uinfo.UserId)
                    {
                        oldmo.GistRemark = mo.GistRemark;
                        oldmo.GistFilename = mo.GistFilename;
                        oldmo.GistLanguage = mo.GistLanguage;
                        oldmo.GistTheme = mo.GistTheme;
                        oldmo.GistContent = mo.GistContent;
                        oldmo.GistContentPreview = mo.GistContentPreview;
                        oldmo.GistRow = mo.GistRow;
                        oldmo.GistOpen = mo.GistOpen;
                        oldmo.GistUpdateTime = DateTime.Now;

                        db.Gist.Update(oldmo);
                        db.SaveChanges();

                        vm.Data = mo.GistCode;
                        vm.Set(SharedEnum.RTag.success);
                    }
                    else
                    {
                        vm.Set(SharedEnum.RTag.fail);
                    }
                }
            }

            return vm;
        }
    }
}
