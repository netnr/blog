using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Netnr.Data;

namespace Netnr.Web.Areas.Doc.Controllers
{
    [Area("Doc")]
    public class ItemController : Controller
    {
        [Description("新增项目")]
        [Authorize]
        [Filters.FilterConfigs.IsValid]
        public IActionResult Add()
        {
            return View();
        }

        [Description("保存项目")]
        [Authorize]
        public string SaveDocSet(Domain.DocSet mo, string savetype)
        {
            string result = "fail";

            var uinfo = new Func.UserAuthAid(HttpContext).Get();

            using (var db = new ContextBase())
            {
                if (savetype == "add")
                {
                    mo.DsCode = Core.UniqueTo.LongId().ToString();
                    mo.Uid = uinfo.UserId;
                    mo.DsStatus = 1;
                    mo.DsCreateTime = DateTime.Now;

                    db.DocSet.Add(mo);
                }
                else
                {
                    var currmo = db.DocSet.Find(mo.DsCode);
                    if (currmo.Uid != uinfo.UserId)
                    {
                        return "unauthorized";
                    }

                    currmo.DsName = mo.DsName;
                    currmo.DsRemark = mo.DsRemark;
                    currmo.DsOpen = mo.DsOpen;

                    db.DocSet.Update(currmo);
                }
                var num = db.SaveChanges();

                result = num > 0 ? "success" : "fail";
            }
            return result;
        }

        [Description("修改项目")]
        [Authorize]
        public IActionResult Setting()
        {
            string code = RouteData.Values["id"]?.ToString();

            var uinfo = new Func.UserAuthAid(HttpContext).Get();

            using var db = new ContextBase();
            var mo = db.DocSet.Find(code);
            if (mo.Uid == uinfo.UserId)
            {
                return View(mo);
            }
            else
            {
                return Content("unauthorized");
            }
        }

        [Description("删除项目")]
        [Authorize]
        public IActionResult Del()
        {
            string code = RouteData.Values["id"]?.ToString();

            var uinfo = new Func.UserAuthAid(HttpContext).Get();

            using (var db = new ContextBase())
            {
                var mo = db.DocSet.Find(code);
                if (mo.Uid == uinfo.UserId)
                {
                    db.DocSet.Remove(mo);
                    var moDetail = db.DocSetDetail.Where(x => x.DsCode == code).ToList();
                    db.DocSetDetail.RemoveRange(moDetail);
                    db.SaveChanges();

                    return Redirect("/doc/item");
                }
            }
            return Content("Bad");
        }
    }
}