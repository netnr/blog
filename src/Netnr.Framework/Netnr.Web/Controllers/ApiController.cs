using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.ComponentModel;
using Netnr.Func.ViewModel;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Cors;

namespace Netnr.Web.Controllers
{
    /// <summary>
    /// 开放接口
    /// </summary>
    public class ApiController : Controller
    {
        [Description("首页列表")]
        public IActionResult Index()
        {
            return View();
        }

        [Description("测试")]
        public IActionResult Test()
        {
            return View();
        }

        #region API 方法

        [HttpGet]
        [Description("获取GUID")]
        public ActionResultVM API81(int? count = 10)
        {
            var vm = new ActionResultVM();

            try
            {
                var list = new List<string>();
                for (int i = 0; i < count; i++)
                {
                    list.Add(Guid.NewGuid().ToString());
                }
                vm.data = list;
                vm.Set(ARTag.success);
            }
            catch (Exception ex)
            {
                vm.Set(ex);
                Core.ConsoleTo.Log(ex);
            }

            return vm;
        }

        [HttpGet]
        [Description("获取GUID To long")]
        public ActionResultVM API82(int? count = 10)
        {
            var vm = new ActionResultVM();

            try
            {
                var list = new List<long>();
                for (int i = 0; i < count; i++)
                {
                    list.Add(Core.UniqueTo.LongId());
                }
                vm.data = list;
                vm.Set(ARTag.success);
            }
            catch (Exception ex)
            {
                vm.Set(ex);
                Core.ConsoleTo.Log(ex);
            }

            return vm;
        }

        /// <summary>
        /// 公共上传
        /// </summary>
        /// <param name="form">表单</param>
        /// <param name="cp">自定义路径，如：static/draw</param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        [HttpOptions]
        [Description("公共上传文件")]
        public ActionResultVM API98([FromForm] IFormCollection form, string cp = null)
        {
            var vm = new ActionResultVM();

            try
            {
                var files = form.Files;
                if (files.Count > 0)
                {
                    var file = files[0];

                    int maxsize = GlobalTo.GetValue<int>("APIOut:API98:MaxSize");
                    if (file.Length > 1024 * 1024 * maxsize)
                    {
                        vm.code = 1;
                        vm.msg = maxsize + " MB max per file";
                    }
                    else
                    {
                        var now = DateTime.Now;
                        string filename = now.ToString("HHmmss") + Guid.NewGuid().ToString("N").Substring(25, 4);
                        string ext = file.FileName.Substring(file.FileName.LastIndexOf('.'));

                        if (ext.ToLower() == ".exe")
                        {
                            vm.code = 2;
                            vm.msg = "Unsupported file format：" + ext;
                        }
                        else
                        {
                            //自定义路径
                            if (!string.IsNullOrWhiteSpace(cp))
                            {
                                cp = cp.TrimStart('/').TrimEnd('/') + '/';
                            }

                            var path = cp + now.ToString("yyyy/MM/dd/");
                            var rootdir = GlobalTo.WebRootPath + "/" + (GlobalTo.GetValue("APIOut:API98:RootDir").TrimStart('/').TrimEnd('/') + "/");
                            string fullpath = rootdir + path;

                            if (!Directory.Exists(fullpath))
                            {
                                Directory.CreateDirectory(fullpath);
                            }

                            using (var fs = new FileStream(fullpath + filename + ext, FileMode.CreateNew))
                            {
                                file.CopyTo(fs);
                                fs.Flush();
                            }

                            var FilePath = path + filename + ext;

                            var jo = new JObject
                            {
                                ["server"] = GlobalTo.GetValue("APIOut:API98:Server").TrimEnd('/') + '/',
                                ["path"] = FilePath
                            };

                            vm.data = jo;

                            vm.Set(ARTag.success);
                        }
                    }
                }
                else
                {
                    vm.Set(ARTag.lack);
                }
            }
            catch (Exception ex)
            {
                vm.Set(ex);
                Core.ConsoleTo.Log(ex);
            }

            return vm;
        }

        [Description("系统错误码说明")]
        public ActionResultVM API9999()
        {
            var vm = new ActionResultVM();

            try
            {
                var dic = new Dictionary<int, string>();
                foreach (ARTag item in Enum.GetValues(typeof(ARTag)))
                {
                    dic.Add((int)item, item.ToString());
                }
                vm.data = dic;
                vm.Set(ARTag.success);
            }
            catch (Exception ex)
            {
                vm.Set(ex);
                Core.ConsoleTo.Log(ex);
            }

            return vm;
        }

        #endregion
    }
}