﻿using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace Netnr.Web.Areas.Gist.Controllers
{
    [Area("Gist")]
    public class DiscoverController : Controller
    {
        [Description("Gist列表")]
        public IActionResult Index(string q, string lang, int page = 1)
        {
            var uinfo = new Func.UserAuthAid(HttpContext).Get();

            var ps = Func.Common.GistQuery(q, lang, 0, uinfo.UserId, page);
            ps.Route = Request.Path;
            ViewData["lang"] = lang;
            ViewData["q"] = q;
            return View("_PartialGistList", ps);
        }
    }
}