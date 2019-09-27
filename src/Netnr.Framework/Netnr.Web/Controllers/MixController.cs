using System.ComponentModel;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Netnr.Web.Filters;

namespace Netnr.Web.Controllers
{
    /// <summary>
    /// 混合、综合、其它
    /// </summary>
    public class MixController : Controller
    {
        [Description("关于页面")]
        public IActionResult About(string ishttp)
        {
            if (!string.IsNullOrWhiteSpace(ishttp))
            {
                return Content("about");
            }

            return View();
        }

        [Description("服务器状态")]
        [ValidateAntiForgeryToken]
        [ResponseCache(Duration = 5)]
        public string AboutServerStatus()
        {
            string url = GlobalTo.GetValue("ServiceApi:ServiceInfo");
            var hwr = Core.HttpTo.HWRequest(url);
            hwr.UserAgent = GlobalTo.GetValue("UserAgent");
            var result = Core.HttpTo.Url(hwr);

            //处理敏感信息
            var rj = result.ToJObject();
            var removeNodes = "available_isos email ip_addresses node_ip ptr ssh_port ve_mac1".Split(" ").ToList();
            foreach (var item in removeNodes)
            {
                if (rj.ContainsKey(item))
                {
                    rj.Remove(item);
                }
            }
            result = rj.ToJson();

            return result;
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
    }
}