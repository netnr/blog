using System.ComponentModel;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Netnr.Func.ViewModel;
using Netnr.Web.Filters;

namespace Netnr.Web.Controllers
{
    /// <summary>
    /// 混合、综合、其它
    /// </summary>
    public class MixController : Controller
    {
        [Description("关于页面")]
        public IActionResult About()
        {
            return View();
        }

        [Description("服务器状态")]
        [ValidateAntiForgeryToken]
        [ResponseCache(Duration = 10)]
        public ActionResultVM AboutServerStatus()
        {
            var vm = new ActionResultVM
            {
                data = new Fast.OSInfoTo()
            };

            return vm;
        }

        [Description("日志页面")]
        [FilterConfigs.LocalAuth]
        public IActionResult Log()
        {
            return View();
        }

        [ResponseCache(Duration = 10)]
        [Description("查询日志")]
        public string QueryLog(int page, int rows)
        {
            return Func.LogsAid.Query(page, rows);
        }

        [Description("日志图表")]
        [FilterConfigs.LocalAuth]
        public IActionResult LogChart()
        {
            return View();
        }

        [ResponseCache(Duration = 10)]
        [Description("查询日志流量")]
        public string QueryLogReportFlow(int? type)
        {
            return Func.LogsAid.ReportFlow(type ?? 0);
        }

        [ResponseCache(Duration = 10)]
        [Description("查询日志Top")]
        public string QueryLogReportTop(int? type, string field)
        {
            return Func.LogsAid.ReportTop(type ?? 0, field);
        }

        [Description("条款")]
        public IActionResult Terms()
        {
            return View();
        }

        [Description("FAQ")]
        public IActionResult FAQ()
        {
            return View();
        }
    }
}