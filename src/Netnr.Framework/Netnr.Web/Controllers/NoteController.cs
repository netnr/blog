using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Netnr.Web.Controllers
{
    public class NoteController : Controller
    {
        [Description("记事本")]
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
    }
}