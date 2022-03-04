using Microsoft.AspNetCore.Authorization;
using Netnr.Blog.Data;

namespace Netnr.Blog.Web.Areas.Doc.Controllers
{
    [Area("Doc")]
    public class ItemController : Controller
    {
        public ContextBase db;

        public ItemController(ContextBase cb)
        {
            db = cb;
        }

        /// <summary>
        /// 新增项目
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Apps.FilterConfigs.IsCompleteInfo]
        public IActionResult Add()
        {
            return View("_PartialItemForm");
        }

        /// <summary>
        /// 修改项目
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public IActionResult Setting()
        {
            string code = RouteData.Values["id"]?.ToString();

            var uinfo = Apps.LoginService.Get(HttpContext);

            var mo = db.DocSet.Find(code);
            if (mo.Uid == uinfo.UserId)
            {
                return View("_PartialItemForm", mo);
            }
            else
            {
                return Content("unauthorized");
            }
        }

        /// <summary>
        /// 保存项目
        /// </summary>
        /// <param name="mo"></param>
        /// <returns></returns>
        [Authorize]
        public SharedResultVM SaveDocSet(Domain.DocSet mo)
        {
            var vm = new SharedResultVM();

            var uinfo = Apps.LoginService.Get(HttpContext);

            if (string.IsNullOrWhiteSpace(mo.DsCode))
            {
                mo.DsCode = Core.UniqueTo.LongId().ToString();
                mo.Uid = uinfo.UserId;
                mo.DsStatus = 1;
                mo.DsCreateTime = DateTime.Now;

                db.DocSet.Add(mo);

                var num = db.SaveChanges();
                vm.Set(num > 0);
            }
            else
            {
                var currmo = db.DocSet.Find(mo.DsCode);
                if (currmo.Uid != uinfo.UserId)
                {
                    vm.Set(SharedEnum.RTag.unauthorized);
                }
                else
                {
                    currmo.DsName = mo.DsName;
                    currmo.DsRemark = mo.DsRemark;
                    currmo.DsOpen = mo.DsOpen;
                    currmo.Spare1 = mo.Spare1;

                    db.DocSet.Update(currmo);

                    var num = db.SaveChanges();
                    vm.Set(num > 0);
                }
            }
            return vm;
        }

        /// <summary>
        /// 删除项目
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public IActionResult Del()
        {
            string code = RouteData.Values["id"]?.ToString();

            var uinfo = Apps.LoginService.Get(HttpContext);

            var mo = db.DocSet.Find(code);
            if (mo.Uid == uinfo.UserId)
            {
                db.DocSet.Remove(mo);
                var moDetail = db.DocSetDetail.Where(x => x.DsCode == code).ToList();
                db.DocSetDetail.RemoveRange(moDetail);
                db.SaveChanges();

                return Redirect("/doc/user/" + uinfo.UserId);
            }

            return Content("Bad");
        }
    }
}