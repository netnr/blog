using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Netnr.Func
{
    /// <summary>
    /// 日志
    /// </summary>
    public class LogsAid
    {
        /// <summary>
        /// 路径
        /// </summary>
        public static readonly string dbPath = GlobalTo.GetValue("logs:path").Replace("~", GlobalTo.ContentRootPath);

        /// <summary>
        /// 分批写入满足的条件：缓存的日志数量
        /// </summary>
        public static readonly int cacheLogCount = GlobalTo.GetValue<int>("logs:batchwritecount");

        /// <summary>
        /// 分批写入满足的条件：缓存的时长，单位秒
        /// </summary>
        public static readonly int cacheLogTime = GlobalTo.GetValue<int>("logs:batchwritetime");

        /// <summary>
        /// 日志
        /// </summary>
        public class LogsVM
        {
            /// <summary>
            /// ID
            /// </summary>
            [PrimaryKey, AutoIncrement]
            public int LogId { get; set; }
            /// <summary>
            /// 账号
            /// </summary>
            public string LogName { get; set; }
            /// <summary>
            /// 昵称
            /// </summary>
            public string LogNickname { get; set; }
            /// <summary>
            /// 请求ID
            /// </summary>
            public string LogRequestId { get; set; }
            /// <summary>
            /// 动作
            /// </summary>
            [Indexed]
            public string LogAction { get; set; }
            /// <summary>
            /// 内容
            /// </summary>
            public string LogContent { get; set; }
            /// <summary>
            /// 链接
            /// </summary>
            public string LogUrl { get; set; }
            /// <summary>
            /// IP
            /// </summary>
            public string LogIp { get; set; }
            /// <summary>
            /// 引荐
            /// </summary>
            public string LogReferer { get; set; }
            /// <summary>
            /// 创建时间
            /// </summary>
            [Indexed]
            public DateTime LogCreateTime { get; set; }
            /// <summary>
            /// 浏览器
            /// </summary>
            public string LogBrowserName { get; set; }
            /// <summary>
            /// 操作系统
            /// </summary>
            public string LogSystemName { get; set; }
            /// <summary>
            /// 分组
            /// </summary>
            public int LogGroup { get; set; }
            /// <summary>
            /// 级别
            /// </summary>
            public string LogLevel { get; set; }
            /// <summary>
            /// 备注
            /// </summary>
            public string LogRemark { get; set; }
        }

        /// <summary>
        /// 新增（间隔分批写入）
        /// </summary>
        /// <param name="mo"></param>
        public static void Insert(LogsVM mo)
        {
            try
            {
                //日志记录
                var cacheLogsKey = "Global_Logs";
                //上次写入的时间
                var cacheLogWriteKey = "Global_Logs_Write";

                if (!(Core.CacheTo.Get(cacheLogsKey) is List<LogsVM> cacheLogs))
                {
                    cacheLogs = new List<LogsVM>();
                }
                cacheLogs.Add(mo);

                var cacheLogWrite = Core.CacheTo.Get(cacheLogWriteKey) as DateTime?;
                if (!cacheLogWrite.HasValue)
                {
                    cacheLogWrite = DateTime.Now;
                }

                if (cacheLogs?.Count > cacheLogCount || DateTime.Now.ToTimestamp() - cacheLogWrite.Value.ToTimestamp() > cacheLogTime)
                {
                    InsertNow(cacheLogs);

                    cacheLogs = null;
                    cacheLogWrite = DateTime.Now;
                }

                Core.CacheTo.Set(cacheLogsKey, cacheLogs, 3600 * 24 * 30);
                Core.CacheTo.Set(cacheLogWriteKey, cacheLogWrite, 3600 * 24 * 30);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="logs"></param>
        public static void InsertNow(List<LogsVM> logs)
        {
            try
            {
                using var db = new SQLiteConnection(dbPath);
                db.CreateTable<LogsVM>();
                db.InsertAll(logs);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        public static string Query(int page, int rows)
        {
            using var db = new SQLiteConnection(dbPath);
            var query = db.Table<LogsVM>();

            var total = query.Count();
            var data = query.OrderByDescending(x => x.LogCreateTime).Skip((page - 1) * rows).Take(rows).ToList();

            return new
            {
                data,
                total
            }.ToJson();
        }

        /// <summary>
        /// 统计流量
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string ReportFlow(int type)
        {
            string result = string.Empty;

            using var db = new SQLiteConnection(dbPath);
            var query = db.Table<LogsVM>();

            var now = DateTime.Now;

            switch (type)
            {
                //今天
                case 0:
                    {
                        query = query.Where(x => x.LogCreateTime >= now.Date);

                        var list = query.Select(x => new
                        {
                            x.LogCreateTime,
                            x.LogIp
                        }).GroupBy(x => x.LogCreateTime.ToString("yyyy-MM-dd HH")).OrderByDescending(x => x.Key).Select(x => new
                        {
                            time = x.Key.Split(' ')[1] + ":00",
                            pv = x.Count(),
                            ip = x.Select(p => p.LogIp).Distinct().Count()
                        }).ToList();

                        result = list.ToJson();
                    }
                    break;
                //昨天
                case -1:
                    {
                        var yday = now.AddDays(-1);
                        query = query.Where(x => x.LogCreateTime < now.Date && x.LogCreateTime >= yday.Date);

                        var list = query.Select(x => new
                        {
                            x.LogCreateTime,
                            x.LogIp
                        }).GroupBy(x => x.LogCreateTime.ToString("yyyy-MM-dd HH")).OrderByDescending(x => x.Key).Select(x => new
                        {
                            time = x.Key.Split(' ')[1] + ":00",
                            pv = x.Count(),
                            ip = x.Select(p => p.LogIp).Distinct().Count()
                        }).ToList();

                        result = list.ToJson();
                    }
                    break;
                //最近
                default:
                    {
                        var old = now.AddDays(-type);
                        query = query.Where(x => x.LogCreateTime >= old.Date);

                        var list = query.Select(x => new
                        {
                            x.LogCreateTime,
                            x.LogIp
                        }).GroupBy(x => x.LogCreateTime.Date).OrderByDescending(x => x.Key).Select(x => new
                        {
                            time = x.Key.ToString("yyyy-MM-dd"),
                            pv = x.Count(),
                            ip = x.Select(p => p.LogIp).Distinct().Count()
                        }).ToList();

                        result = list.ToJson();
                    }
                    break;
            }

            return result;
        }

        /// <summary>
        /// 统计属性
        /// </summary>
        /// <param name="type"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static string ReportTop(int type, string field)
        {
            string result = string.Empty;

            using var db = new SQLiteConnection(dbPath);
            var query = db.Table<LogsVM>();

            var now = DateTime.Now;

            switch (type)
            {
                case 0:
                    query = query.Where(x => x.LogCreateTime >= now.Date);
                    break;
                case -1:
                    var yday = now.AddDays(-1);
                    query = query.Where(x => x.LogCreateTime < now.Date && x.LogCreateTime >= yday.Date);
                    break;
                default:
                    var old = now.AddDays(-type);
                    query = query.Where(x => x.LogCreateTime >= old.Date);
                    break;
            }

            switch (field)
            {
                case "url":
                    {
                        var list = query.GroupBy(x => x.LogUrl).Select(x => new
                        {
                            url = x.Key,
                            pv = x.Count()
                        }).OrderByDescending(x => x.pv).Take(20).ToList();

                        result = list.ToJson();
                    }
                    break;

                case "referer":
                    {
                        var list = query.GroupBy(x => x.LogReferer).Select(x => new
                        {
                            url = x.Key,
                            pv = x.Count()
                        }).OrderByDescending(x => x.pv).Take(20).ToList();
                        result = list.ToJson();
                    }
                    break;

                default:
                    result = "[]";
                    break;
            }

            return result;
        }
    }
}
