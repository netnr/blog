using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Netnr.Data;

namespace Netnr.Web.Areas.Run.Controllers
{
    [Area("Run")]
    public class HomeController : Controller
    {
        /// <summary>
        /// Run首页
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View("_PartialMonacoEditor");
        }

        /// <summary>
        /// Run预览
        /// </summary>
        /// <returns></returns>
        public IActionResult Preview()
        {
            return View();
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="mo"></param>
        /// <returns></returns>
        [Authorize]
        public ActionResultVM SaveRun(Domain.Run mo)
        {
            var vm = new ActionResultVM();

            var uinfo = new Func.UserAuthAid(HttpContext).Get();

            using (var db = new ContextBase())
            {
                //add
                if (string.IsNullOrWhiteSpace(mo.RunCode))
                {
                    mo.RunId = Guid.NewGuid().ToString();
                    mo.RunCreateTime = DateTime.Now;
                    mo.RunStatus = 1;
                    mo.RunOpen = 1;
                    mo.Uid = uinfo.UserId;

                    mo.RunCode = Core.UniqueTo.LongId().ToString();
                    db.Run.Add(mo);
                    int num = db.SaveChanges();

                    vm.data = mo.RunCode;
                    vm.Set(num > 0);
                }
                else
                {
                    var oldmo = db.Run.Where(x => x.RunCode == mo.RunCode).FirstOrDefault();

                    if (oldmo != null)
                    {
                        if (oldmo.Uid == uinfo.UserId)
                        {
                            oldmo.RunContent1 = mo.RunContent1;
                            oldmo.RunContent2 = mo.RunContent2;
                            oldmo.RunContent3 = mo.RunContent3;
                            oldmo.RunRemark = mo.RunRemark;
                            oldmo.RunTheme = mo.RunTheme;

                            db.Run.Update(oldmo);
                            int num = db.SaveChanges();

                            vm.data = mo.RunCode;
                            vm.Set(num > 0);
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
