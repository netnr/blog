using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Netnr.Data;

namespace Netnr.Web.Areas.Gist.Controllers
{
    [Area("Gist")]
    public class HomeController : Controller
    {
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
        public ActionResultVM SaveGist(Domain.Gist mo)
        {
            var vm = new ActionResultVM();

            var uinfo = new Func.UserAuthAid(HttpContext).Get();
            using (var db = new ContextBase())
            {
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

                    vm.data = mo.GistCode;
                    vm.Set(ARTag.success);
                }
                else
                {
                    var oldmo = db.Gist.Where(x => x.GistCode == mo.GistCode).FirstOrDefault();
                    if (oldmo != null)
                    {
                        if (oldmo.Uid == uinfo.UserId)
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

                            vm.data = mo.GistCode;
                            vm.Set(ARTag.success);
                        }
                        else
                        {
                            vm.Set(ARTag.refuse);
                        }
                    }
                    else
                    {
                        vm.Set(ARTag.invalid);
                    }
                }
            }

            return vm;
        }
    }
}
