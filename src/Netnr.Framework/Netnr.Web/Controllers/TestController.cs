using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System;
using System.Text.RegularExpressions;

namespace Netnr.Web.Controllers
{
    public class TestController : Controller
    {
        public IActionResult Index()
        {
            var result = string.Empty;


            return Content(result);
        }
    }
}