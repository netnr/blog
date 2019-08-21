﻿using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;

namespace Netnr.Web.Areas.Doc.Controllers
{
    [Area("Doc")]
    public class DiscoverController : Controller
    {
        [Description("项目列表")]
        [ResponseCache(Duration = 10)]
        public IActionResult Index(string q, int page = 1)
        {
            var uinfo = new Func.UserAuthAid(HttpContext).Get();

            var ps = Func.Common.DocQuery(q, 0, uinfo.UserId, page);
            ps.Route = Request.Path;
            ViewData["q"] = q;
            return View("_PartialDocList", ps);
        }
    }
}