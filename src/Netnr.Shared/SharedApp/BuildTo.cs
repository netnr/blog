#if Full || App

using System.Collections.Concurrent;
using Netnr.Core;
using Netnr.SharedFast;

namespace Netnr.SharedApp
{
    /// <summary>
    /// 构建
    /// </summary>
    public class BuildTo
    {
        private readonly HttpContext Context;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="httpContext"></param>
        public BuildTo(HttpContext httpContext)
        {
            Context = httpContext;
        }

        /// <summary>
        /// 根据控制器构建静态页面
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public SharedResultVM Html<T>() where T : Controller
        {
            var vm = new SharedResultVM();

            try
            {
                //访问前缀
                var urlPrefix = $"{Context.Request.Scheme}://{Context.Request.Host}/home/";
                //保存目录
                var savePath = GlobalTo.WebRootPath + "/";

                //反射action
                var type = typeof(T);
                var methods = type.GetMethods().Where(x => x.DeclaringType == type).ToList();

                vm.Log.Add($"Build Count：{methods.Count}");

                var cbs = new ConcurrentBag<string>();
                //并行请求
                Parallel.ForEach(methods, mh =>
                {
                    Console.WriteLine(mh.Name);

                    cbs.Add(mh.Name);
                    string html = HttpTo.Get(urlPrefix + mh.Name);
                    FileTo.WriteText(html, savePath + mh.Name.ToLower() + ".html", false);
                });
                vm.Log.AddRange(cbs);
                Console.WriteLine("\nDone!\n");

                vm.Set(SharedEnum.RTag.success);
            }
            catch (Exception ex)
            {
                vm.Set(ex);
            }

            return vm;
        }
    }
}

#endif