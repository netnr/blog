using Microsoft.AspNetCore.Authorization;
using Netnr.Blog.Data;

namespace Netnr.Blog.Web.Areas.Run.Controllers
{
    [Area("Run")]
    public class HomeController : Controller
    {
        public ContextBase db;

        public HomeController(ContextBase cb)
        {
            db = cb;
        }

        /// <summary>
        /// Run首页
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
        public SharedResultVM SaveRun(Domain.Run mo)
        {
            var vm = Apps.LoginService.CompleteInfoValid(HttpContext);
            if (vm.Code == 200)
            {
                var uinfo = Apps.LoginService.Get(HttpContext);

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

                    vm.Data = mo.RunCode;
                    vm.Set(num > 0);
                }
                else
                {
                    var oldmo = db.Run.FirstOrDefault(x => x.RunCode == mo.RunCode);
                    if (oldmo?.Uid == uinfo.UserId)
                    {
                        oldmo.RunContent1 = mo.RunContent1;
                        oldmo.RunContent2 = mo.RunContent2;
                        oldmo.RunContent3 = mo.RunContent3;
                        oldmo.RunRemark = mo.RunRemark;
                        oldmo.RunTheme = mo.RunTheme;

                        db.Run.Update(oldmo);
                        int num = db.SaveChanges();

                        vm.Data = mo.RunCode;
                        vm.Set(num > 0);
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
