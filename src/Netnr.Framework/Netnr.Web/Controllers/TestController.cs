using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Netnr.Web.Controllers
{
    public class TestController : Controller
    {
        public IActionResult Index()
        {
            var result = string.Empty;

            try
            {

            }
            catch (Exception)
            {
            }

            return Content(result);
        }
    }
}