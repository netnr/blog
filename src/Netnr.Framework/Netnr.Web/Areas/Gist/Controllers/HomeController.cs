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
        [Description("Gist首页")]
        public IActionResult Index()
        {
            return View("_PartialMonacoEditor");
        }

        [Description("保存")]
        [Authorize]
        public IActionResult SaveGist(Domain.Gist mo)
        {
            string result = "fail";

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
                    result = new
                    {
                        code = mo.GistCode,
                        message = "success"
                    }.ToJson();
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
                            result = new
                            {
                                code = mo.GistCode,
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
