using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Netnr.Func.ViewModel;
using Newtonsoft.Json.Linq;

namespace Netnr.Web.Controllers
{
    /// <summary>
    /// 公共接口
    /// </summary>
    [Route("api/v1/[action]")]
    [Filters.FilterConfigs.AllowCors]
    public partial class APIController : ControllerBase
    {
        /// <summary>
        /// 登录用户个人信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResultVM UserInfo()
        {
            var vm = new ActionResultVM();

            try
            {
                var uinfo = new Func.UserAuthAid(HttpContext).Get();
                if (uinfo.UserId != 0)
                {
                    vm.data = uinfo;

                    vm.Set(ARTag.success);
                }
                else
                {
                    vm.Set(ARTag.unauthorized);
                }
            }
            catch (Exception ex)
            {
                vm.Set(ex);
                Core.ConsoleTo.Log(ex);
            }

            return vm;
        }

        /// <summary>
        /// 获取GUID
        /// </summary>
        /// <param name="count">条数，默认10</param>
        /// <returns></returns>
        [HttpGet]
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

        /// <summary>
        /// 获取GUID To long
        /// </summary>
        /// <param name="count">条数，默认10</param>
        /// <returns></returns>
        [HttpGet]
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
        /// <param name="file">文件</param>
        /// <param name="cp">可选，自定义路径，如：static/draw</param>
        /// <returns></returns>
        [HttpPost]
        [HttpOptions]
        public ActionResultVM API98(IFormFile file, string cp = null)
        {
            var vm = new ActionResultVM();

            if (Request.Method == "OPTIONS")
            {
                vm.Set(ARTag.success);
                return vm;
            }

            try
            {
                if (file != null)
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
                        var rootdir = GlobalTo.WebRootPath + "/" + (GlobalTo.GetValue("StaticResource:RootDir") + "/");
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

                        var jo = new
                        {
                            server = GlobalTo.GetValue("StaticResource:Server").TrimEnd('/') + '/',
                            path = FilePath
                        };

                        vm.data = jo;

                        vm.Set(ARTag.success);
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

        /// <summary>
        /// 系统错误码说明
        /// </summary>
        /// <returns></returns>
        [HttpGet]
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
    }
}