using SQLite;
using System;
using Netnr.FileServer.Model;
using System.Collections.Generic;

namespace Netnr.FileServer.Base
{
    /// <summary>
    /// 数据库操作
    /// </summary>
    public class SQLiteBase
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        public static string SQLiteConn = GlobalTo.GetValue("ConnectionStrings:SQLiteConn").Replace("~", GlobalTo.ContentRootPath);

        /// <summary>
        /// 创建App
        /// </summary>
        /// <returns></returns>
        public static ActionResultVM CreateApp()
        {
            var vm = new ActionResultVM();

            try
            {
                using var db = new SQLiteConnection(SQLiteConn);
                db.CreateTable<SysKey>();

                var mo = new SysKey()
                {
                    SkAppId = Core.UniqueTo.LongId().ToString(),
                    SkAppKey = Core.UniqueTo.LongId().ToString() + Core.UniqueTo.LongId().ToString(),
                    SkCreateTime = DateTime.Now,
                    SkName = "默认",
                    SkToken = Core.CalcTo.MD5(Core.UniqueTo.LongId().ToString()),
                    SkTokenExpireTime = DateTime.Now.AddMinutes(GlobalTo.GetValue<int>("Safe:TokenExpired")),
                    SkRemark = "系统自动生成"
                };

                int num = db.Insert(mo);

                vm.Set(num > 0);
                vm.data = mo;
            }
            catch (Exception ex)
            {
                vm.Set(ex);
            }

            return vm;
        }

        /// <summary>
        /// 获取Token
        /// </summary>
        /// <param name="AppId">分配的应用ID</param>
        /// <param name="AppKey">分配的应用密钥</param>
        /// <returns></returns>
        public static ActionResultVM GetToken(string AppId, string AppKey)
        {
            var vm = new ActionResultVM();

            try
            {
                using var db = new SQLiteConnection(SQLiteConn);
                db.CreateTable<SysKey>();
                var sk = db.Table<SysKey>().FirstOrDefault(x => x.SkAppId == AppId && x.SkAppKey == AppKey);
                if (sk != null)
                {
                    sk.SkToken = Core.CalcTo.MD5(Core.UniqueTo.LongId().ToString());
                    sk.SkTokenExpireTime = DateTime.Now.AddMinutes(GlobalTo.GetValue<int>("Safe:TokenExpired"));

                    int num = db.Update(sk);
                    vm.data = sk;
                    vm.Set(num > 0);
                }
                else
                {
                    vm.Set(ARTag.unauthorized);
                }
            }
            catch (Exception ex)
            {
                vm.Set(ex);
            }

            return vm;
        }

        /// <summary>
        /// 获取App列表
        /// </summary>
        /// <param name="pageNumber">页码，默认1</param>
        /// <param name="pageSize">页量，默认20</param>
        /// <returns></returns>
        public static ActionResultVM GetAppList(int pageNumber = 1, int pageSize = 20)
        {
            var vm = new ActionResultVM();

            try
            {
                using var db = new SQLiteConnection(SQLiteConn);
                db.CreateTable<SysKey>();
                var sk = db.Table<SysKey>().OrderByDescending(x => x.SkCreateTime).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                vm.data = sk;
                vm.Set(ARTag.success);
            }
            catch (Exception ex)
            {
                vm.Set(ex);
            }

            return vm;
        }

        /// <summary>
        /// 验证Token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static ActionResultVM ValidToken(string token)
        {
            var vm = new ActionResultVM();

            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    vm.Set(ARTag.unauthorized);
                }
                else
                {
                    using var db = new SQLiteConnection(SQLiteConn);
                    db.CreateTable<SysKey>();
                    var sk = db.Table<SysKey>().FirstOrDefault(x => x.SkToken == token);

                    vm.Set(sk?.SkTokenExpireTime > DateTime.Now);
                }
            }
            catch (Exception ex)
            {
                vm.Set(ex);
            }

            return vm;
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="list"></param>
        public static ActionResultVM InsertFile(List<FileRecord> list)
        {
            var vm = new ActionResultVM();

            try
            {
                using var db = new SQLiteConnection(SQLiteConn);
                db.CreateTable<FileRecord>();
                int num = db.InsertAll(list);

                vm.Set(num > 0);
            }
            catch (Exception ex)
            {
                vm.Set(ex);
            }

            return vm;
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="path">路径，传ID时需传 isid=true</param>
        /// <param name="isid">是ID</param>
        public static ActionResultVM QueryFile(string path, bool isid = false)
        {
            var vm = new ActionResultVM();

            try
            {
                using var db = new SQLiteConnection(SQLiteConn);
                FileRecord fr = null;

                if (isid)
                {
                    fr = db.Table<FileRecord>().FirstOrDefault(x => x.FrId == path);
                }
                else
                {
                    fr = db.Table<FileRecord>().FirstOrDefault(x => x.FrPath == path);
                }
                if (fr == null)
                {
                    vm.Set(ARTag.lack);
                }
                else
                {
                    vm.Set(ARTag.success);
                    vm.data = fr;
                }
            }
            catch (Exception ex)
            {
                vm.Set(ex);
            }

            return vm;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="path">路径，传ID时需传 isid=true</param>
        /// <param name="isid">是ID</param>
        public static ActionResultVM DeleteFile(string path, bool isid = false)
        {
            var vm = new ActionResultVM();

            try
            {
                using var db = new SQLiteConnection(SQLiteConn);
                FileRecord fr = null;

                if (isid)
                {
                    fr = db.Table<FileRecord>().FirstOrDefault(x => x.FrId == path);
                }
                else
                {
                    fr = db.Table<FileRecord>().FirstOrDefault(x => x.FrPath == path);
                }
                if (fr == null)
                {
                    vm.Set(ARTag.lack);
                }
                else
                {
                    var fp = GlobalTo.WebRootPath + fr.FrPath;
                    if (System.IO.File.Exists(fp))
                    {
                        System.IO.File.Delete(fp);
                    }

                    int num = db.Delete(fr);

                    vm.Set(num > 0);
                }
            }
            catch (Exception ex)
            {
                vm.Set(ex);
            }

            return vm;
        }
    }
}