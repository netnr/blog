using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Netnr.FileServer.Base;
using Netnr.FileServer.Model;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Cors;

namespace Netnr.FileServer.Controllers
{
    /// <summary>
    /// API接口
    /// </summary>
    [Route("[controller]/[action]")]
    [Filters.FilterConfigs.AllowCors]
    public class APIController : ControllerBase
    {
        /// <summary>
        /// 创建App，仅限开发环境调用
        /// </summary>
        /// <param name="password">密码，必填，默认密码：nr</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResultVM CreateApp(string password)
        {
            var vm = new ActionResultVM();

            try
            {
                if (GlobalTo.GetValue<bool>("Safe:IsDev"))
                {
                    if (!string.IsNullOrWhiteSpace(password) && password == GlobalTo.GetValue("Safe:CreateAppPassword"))
                    {
                        vm = SQLiteBase.CreateApp();
                    }
                    else
                    {
                        vm.Set(ARTag.unauthorized);
                        vm.msg = "密码错误";
                    }
                }
                else
                {
                    vm.Set(ARTag.refuse);
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
        /// 获取App列表，仅限开发环境调用
        /// </summary>
        /// <param name="pageNumber">页码，默认1</param>
        /// <param name="pageSize">页量，默认20</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResultVM GetAppList(int pageNumber = 1, int pageSize = 20)
        {
            var vm = new ActionResultVM();

            try
            {
                if (GlobalTo.GetValue<bool>("Safe:IsDev"))
                {
                    vm = SQLiteBase.GetAppList(pageNumber, pageSize);
                }
                else
                {
                    vm.Set(ARTag.refuse);
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
        /// 清空数据库和上传文件，仅限开发环境调用
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResultVM ResetAll(string password)
        {
            var vm = new ActionResultVM();

            try
            {
                if (GlobalTo.GetValue<bool>("Safe:IsDev"))
                {
                    if (!string.IsNullOrWhiteSpace(password) && password == GlobalTo.GetValue("Safe:CreateAppPassword"))
                    {
                        //清空数据库
                        using var db = new SQLite.SQLiteConnection(SQLiteBase.SQLiteConn);
                        db.DeleteAll<SysKey>();
                        db.DeleteAll<FileRecord>();

                        //删除上传文件
                        var rootdir = GlobalTo.WebRootPath + GlobalTo.GetValue("StaticResource:RootDir");
                        if (Directory.Exists(rootdir))
                        {
                            Directory.Delete(rootdir, true);
                        }

                        vm.Set(ARTag.success);
                    }
                    else
                    {
                        vm.Set(ARTag.unauthorized);
                        vm.msg = "密码错误";
                    }
                }
                else
                {
                    vm.Set(ARTag.refuse);
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
        /// 获取Token
        /// </summary>
        /// <param name="AppId">分配的应用ID</param>
        /// <param name="AppKey">分配的应用密钥</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResultVM GetToken(string AppId, string AppKey)
        {
            var vm = new ActionResultVM();

            try
            {
                if (string.IsNullOrWhiteSpace(AppId) || string.IsNullOrWhiteSpace(AppKey))
                {
                    vm.Set(ARTag.lack);
                    vm.msg = "参数缺失";
                }
                else
                {
                    if (!(Core.CacheTo.Get(AppKey) is ActionResultVM cvm))
                    {
                        vm = SQLiteBase.GetToken(AppId, AppKey);

                        if (vm.code == 200)
                        {
                            //Token缓存
                            Core.CacheTo.Set(AppKey, vm, GlobalTo.GetValue<int>("Safe:TokenCache"), false);
                        }
                    }
                    else
                    {
                        vm = cvm;
                    }
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
        /// 上传文件，文件流
        /// </summary>
        /// <param name="file">文件流</param>
        /// <param name="token">token，授权验证，必填</param>
        /// <param name="owner">所属者，一个应用对应一个所属者，必填</param>
        /// <param name="subdir">子目录，可选</param>
        /// <returns></returns>
        [HttpPost]
        [HttpOptions]
        public ActionResultVM Upload(IFormFile file, string token, string owner, string subdir)
        {
            var vm = new ActionResultVM();

            try
            {
                if (file == null)
                {
                    vm.msg = "未找到上传文件";
                }
                else if (string.IsNullOrWhiteSpace(owner))
                {
                    vm.msg = "owner不能为空";
                }
                else
                {
                    owner = owner.Trim();

                    //验证token
                    var vt = SQLiteBase.ValidToken(token);
                    if (vt.code != 200)
                    {
                        vm = vt;
                    }
                    else
                    {
                        var listFr = new List<FileRecord>();

                        var rootdir = GlobalTo.GetValue("StaticResource:RootDir") + owner;
                        if (!string.IsNullOrWhiteSpace(subdir))
                        {
                            rootdir += subdir;
                        }

                        var now = DateTime.Now;

                        var path = $"{rootdir.TrimEnd('/')}/{now.Year}/{now.ToString("MM")}/";
                        var pathMp = GlobalTo.WebRootPath + path;
                        if (!Directory.Exists(pathMp))
                        {
                            Directory.CreateDirectory(pathMp);
                        }

                        {
                            var fr = new FileRecord()
                            {
                                FrId = Core.UniqueTo.LongId().ToString(),
                                FrOwnerUser = owner,
                                FrName = file.FileName,
                                FrSize = file.Length.ToString(),
                                FrType = file.ContentType,
                                FrCreateTime = now
                            };
                            fr.FrName = fr.FrName.Split('\\').LastOrDefault();

                            var filename = fr.FrId + file.FileName.Substring(Math.Max(file.FileName.LastIndexOf('.'), 0));

                            using (var fs = new FileStream(pathMp + filename, FileMode.CreateNew))
                            {
                                file.CopyTo(fs);
                                fs.Flush();
                            }

                            fr.FrPath = path + filename;
                            listFr.Add(fr);
                        }

                        vm = SQLiteBase.InsertFile(listFr);
                        if (vm.code == 200)
                        {
                            if (listFr.Count == 1)
                            {
                                vm.data = listFr.First();
                            }
                            else
                            {
                                vm.data = listFr;
                            }
                        }
                    }
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
        /// 复制文件
        /// </summary>
        /// <param name="token"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResultVM Copy(string token, string path)
        {
            var vm = new ActionResultVM();

            try
            {
                //验证token
                var vt = SQLiteBase.ValidToken(token);
                if (vt.code != 200)
                {
                    vm = vt;
                }
                else
                {
                    var qf = SQLiteBase.QueryFile(path);
                    if (qf.code != 200)
                    {
                        vm = qf;
                    }
                    else
                    {
                        var nowfile = qf.data as FileRecord;

                        var fr = new FileRecord()
                        {
                            FrId = Core.UniqueTo.LongId().ToString(),
                            FrOwnerUser = nowfile.FrOwnerUser,
                            FrName = nowfile.FrName,
                            FrSize = nowfile.FrSize,
                            FrType = nowfile.FrType,
                            FrCreateTime = DateTime.Now
                        };

                        var mppath = GlobalTo.WebRootPath + path;
                        var newp1 = path.Substring(0, path.LastIndexOf('/'));
                        var newp2 = path.Substring(path.LastIndexOf('.'));

                        fr.FrPath = newp1 + "/" + fr.FrId + newp2;

                        var mpnewpath = GlobalTo.WebRootPath + fr.FrPath;

                        System.IO.File.Copy(mppath, mpnewpath);

                        vm = SQLiteBase.InsertFile(new List<FileRecord> { fr });
                        if (vm.code == 200)
                        {
                            vm.data = fr;
                        }
                    }
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
        /// 覆盖文件，文件流
        /// </summary>
        /// <param name="file">文件流</param>
        /// <param name="token">token，授权验证，必填</param>
        /// <param name="path">原路径（任选一）</param>
        /// <param name="id">文件ID（任选一）</param>
        /// <returns></returns>
        [HttpPost]
        [HttpOptions]
        public ActionResultVM Cover(IFormFile file, string token, string path, string id)
        {
            var vm = new ActionResultVM();

            try
            {
                if (file == null)
                {
                    vm.msg = "没找到上传的文件";
                }
                else
                {
                    //验证token
                    var vt = SQLiteBase.ValidToken(token);
                    if (vt.code != 200)
                    {
                        vm = vt;
                    }
                    else
                    {
                        var qf = new ActionResultVM();

                        if (string.IsNullOrWhiteSpace(path))
                        {
                            qf = SQLiteBase.QueryFile(id, true);
                        }
                        else
                        {
                            qf = SQLiteBase.QueryFile(path);
                        }

                        if (qf.code != 200)
                        {
                            vm = qf;
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(path))
                            {
                                path = (qf.data as FileRecord)?.FrPath;
                            }

                            var mp = GlobalTo.WebRootPath + path;
                            if (System.IO.File.Exists(mp))
                            {
                                System.IO.File.Delete(mp);

                                using (var fs = new FileStream(mp, FileMode.CreateNew))
                                {
                                    file.CopyTo(fs);
                                    fs.Flush();
                                }

                                vm.Set(ARTag.success);
                            }
                            else
                            {
                                vm.Set(ARTag.invalid);
                                vm.msg = "文件路径无效";
                            }
                        }
                    }
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
        /// 删除文件
        /// </summary>
        /// <param name="token">token，授权验证，必填</param>
        /// <param name="path">原路径（任选一）</param>
        /// <param name="id">文件ID（任选一）</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResultVM Delete(string token, string path, string id)
        {
            var vm = new ActionResultVM();

            try
            {
                //验证token
                var vt = SQLiteBase.ValidToken(token);
                if (vt.code != 200)
                {
                    vm = vt;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        vm = SQLiteBase.DeleteFile(id, true);
                    }
                    else
                    {
                        vm = SQLiteBase.DeleteFile(path);
                    }
                }
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
