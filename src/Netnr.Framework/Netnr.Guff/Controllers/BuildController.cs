using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Netnr.Guff.Controllers
{
    public class BuildController : Controller
    {
        /// <summary>
        /// 生成静态文件
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var startNow = DateTime.Now;

            var url = Request.Scheme + "://" + Request.Host.ToString() + "/home/";
            var path = GlobalTo.WebRootPath + "/";
            var pageTotal = 0;

            var listOut = new List<string>();
            var cacheKey = "GlobalKey-HtmlPath";
            Core.CacheTo.Set(cacheKey, "yes");

            //反射action
            var type = typeof(HomeController);
            var methods = type.GetMethods();
            //并行请求
            Parallel.ForEach(methods, mh =>
            {
                if (mh.DeclaringType == type)
                {
                    string html = Core.HttpTo.Get(url + mh.Name);
                    Core.FileTo.WriteText(html, path, mh.Name.ToLower() + ".html", false);
                    pageTotal++;
                }

            });

            Core.CacheTo.Remove(cacheKey);

            listOut.Add("Starting time：" + startNow.ToString("yyyy-MM-dd HH:mm:ss"));

            listOut.Add("Time：" + (DateTime.Now - startNow).TotalMilliseconds + " ms");
            listOut.Add("Count：" + pageTotal);
            listOut.Add("Successful");

            return Content(string.Join(Environment.NewLine, listOut));
        }
    }
}
