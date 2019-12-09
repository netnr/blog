using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Netnr.Web.Controllers
{
    /// <summary>
    /// 测试
    /// </summary>
    public class TestController : Controller
    {
        /// <summary>
        /// 起始页
        /// </summary>
        /// <returns></returns>
        public ActionResultVM Index()
        {
            var vm = new ActionResultVM();

            try
            {
                //TO DO

            }
            catch (Exception ex)
            {
                vm.Set(ex);
                Core.ConsoleTo.Log(ex, true);
            }

            return vm;
        }
    }
}