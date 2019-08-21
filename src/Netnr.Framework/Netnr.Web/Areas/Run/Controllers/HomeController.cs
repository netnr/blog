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
        [Description("Run首页")]
        public IActionResult Index()
        {
            return View("_PartialMonacoEditor");
        }

        [Description("Run预览")]
        public IActionResult Preview()
        {
            return View();
        }

        [Description("保存")]
        [Authorize]
        public IActionResult SaveRun(Domain.Run mo)
        {
            string result = "fail";

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
                    db.SaveChanges();

                    result = new
                    {
                        code = mo.RunCode,
                        message = "success"
                    }.ToJson();
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
                            db.SaveChanges();

                            result = new
                            {
                                code = mo.RunCode,
                                message = "success"
                            }.ToJson();
                        }
                        else
                        {
                            result = new
                            {
                                message = "refuse"
                            }.ToJson();
                        }
                    }
                    else
                    {
                        result = new
                        {
                            message = "undefined"
                        }.ToJson();
                    }
                }
            }

            return Content(result);
        }
    }
}
