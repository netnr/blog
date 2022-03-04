using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Netnr.Blog.Data;
using Netnr.SharedLogging;
using Netnr.SharedFast;

namespace Netnr.Blog.Web.Controllers
{
    /// <summary>
    /// 后台管理
    /// </summary>
    [Authorize]
    [Apps.FilterConfigs.IsAdmin]
    public class AdminController : Controller
    {
        public ContextBase db;

        public AdminController(ContextBase cb)
        {
            db = cb;
        }

        /// <summary>
        /// 后台管理
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }

        #region 文章管理

        /// <summary>
        /// 文章管理
        /// </summary>
        /// <returns></returns>
        public IActionResult Write()
        {
            return View();
        }

        /// <summary>
        /// 查询文章列表
        /// </summary>
        /// <param name="ivm"></param>
        /// <returns></returns>
        public QueryDataOutputVM QueryWriteList(QueryDataInputVM ivm)
        {
            var ovm = new QueryDataOutputVM();

            var query = from a in db.UserWriting
                        join b in db.UserInfo on a.Uid equals b.UserId
                        select new
                        {
                            a.UwId,
                            a.UwTitle,
                            a.UwCreateTime,
                            a.UwUpdateTime,
                            a.UwReadNum,
                            a.UwReplyNum,
                            a.UwOpen,
                            a.UwStatus,
                            a.UwLaud,
                            a.UwMark,
                            a.UwCategory,

                            b.UserId,
                            b.Nickname,
                            b.UserName,
                            b.UserMail
                        };

            if (!string.IsNullOrWhiteSpace(ivm.Pe1))
            {
                query = GlobalTo.TDB switch
                {
                    SharedEnum.TypeDB.SQLite => query.Where(x => EF.Functions.Like(x.UwTitle, $"%{ivm.Pe1}%")),
                    SharedEnum.TypeDB.PostgreSQL => query.Where(x => EF.Functions.ILike(x.UwTitle, $"%{ivm.Pe1}%")),
                    _ => query.Where(x => x.UwTitle.Contains(ivm.Pe1)),
                };
            }

            Application.CommonService.QueryJoin(query, ivm, ref ovm);

            return ovm;
        }

        /// <summary>
        /// 保存一篇文章（管理）
        /// </summary>
        /// <param name="mo"></param>
        /// <returns></returns>        
        public SharedResultVM WriteAdminSave(Domain.UserWriting mo)
        {
            var vm = new SharedResultVM();

            var uinfo = Apps.LoginService.Get(HttpContext);

            var oldmo = db.UserWriting.FirstOrDefault(x => x.UwId == mo.UwId);

            if (oldmo != null)
            {
                oldmo.UwStatus = mo.UwStatus;
                oldmo.UwReplyNum = mo.UwReplyNum;
                oldmo.UwReadNum = mo.UwReadNum;
                oldmo.UwLaud = mo.UwLaud;
                oldmo.UwMark = mo.UwMark;
                oldmo.UwOpen = mo.UwOpen;

                db.UserWriting.Update(oldmo);

                int num = db.SaveChanges();

                vm.Set(num > 0);
            }

            return vm;
        }

        #endregion

        #region 回复管理

        /// <summary>
        /// 回复管理
        /// </summary>
        /// <returns></returns>
        public IActionResult Reply()
        {
            return View();
        }

        /// <summary>
        /// 查询回复列表
        /// </summary>
        /// <param name="ivm"></param>
        /// <returns></returns>
        public QueryDataOutputVM QueryReplyList(QueryDataInputVM ivm)
        {
            var ovm = new QueryDataOutputVM();

            var query = from a in db.UserReply
                        join b1 in db.UserInfo on a.Uid equals b1.UserId into bg
                        from b in bg.DefaultIfEmpty()
                        select new
                        {
                            a.UrId,
                            a.Uid,
                            a.UrAnonymousName,
                            a.UrAnonymousLink,
                            a.UrAnonymousMail,
                            a.UrTargetType,
                            a.UrTargetId,
                            a.UrContent,
                            a.UrContentMd,
                            a.UrCreateTime,
                            a.UrStatus,
                            a.UrTargetPid,
                            a.Spare1,
                            a.Spare2,
                            a.Spare3,

                            UserId = b == null ? 0 : b.UserId,
                            Nickname = b == null ? null : b.Nickname,
                            UserName = b == null ? null : b.UserName,
                            UserMail = b == null ? null : b.UserMail
                        };

            if (!string.IsNullOrWhiteSpace(ivm.Pe1))
            {
                query = GlobalTo.TDB switch
                {
                    SharedEnum.TypeDB.SQLite => query.Where(x => EF.Functions.Like(x.UrContent, $"%{ivm.Pe1}%")),
                    SharedEnum.TypeDB.PostgreSQL => query.Where(x => EF.Functions.ILike(x.UrContent, $"%{ivm.Pe1}%")),
                    _ => query.Where(x => x.UrContent.Contains(ivm.Pe1)),
                };
            }

            Application.CommonService.QueryJoin(query, ivm, ref ovm);

            return ovm;
        }

        /// <summary>
        /// 保存一条回复（管理）
        /// </summary>
        /// <param name="mo"></param>
        /// <returns></returns>
        public SharedResultVM ReplyAdminSave(Domain.UserReply mo)
        {
            var vm = new SharedResultVM();

            var uinfo = Apps.LoginService.Get(HttpContext);

            var oldmo = db.UserReply.FirstOrDefault(x => x.UrId == mo.UrId);

            if (oldmo != null)
            {
                oldmo.UrAnonymousName = mo.UrAnonymousName;
                oldmo.UrAnonymousMail = mo.UrAnonymousMail;
                oldmo.UrAnonymousLink = mo.UrAnonymousLink;

                oldmo.UrStatus = mo.UrStatus;

                db.UserReply.Update(oldmo);

                int num = db.SaveChanges();

                vm.Set(num > 0);
            }

            return vm;
        }

        #endregion

        #region 日志

        /// <summary>
        /// 日志页面
        /// </summary>
        /// <returns></returns>l
        public IActionResult Log()
        {
            return View();
        }

        /// <summary>
        /// 查询日志
        /// </summary>
        /// <param name="page">页码</param>
        /// <param name="rows">行数</param>
        /// <param name="wheres">条件</param>
        /// <returns></returns>
        [ResponseCache(Duration = 10)]
        public string QueryLog(int page, int rows, string wheres)
        {
            var now = DateTime.Now;
            List<List<string>> listWhere = new();
            try
            {
                if (!string.IsNullOrWhiteSpace(wheres))
                {
                    var jws = JArray.Parse(wheres);
                    foreach (var wi in jws)
                    {
                        var w1 = wi[0]?.ToString();
                        var w2 = wi[1]?.ToString();
                        var w3 = wi[2]?.ToString();
                        if (!string.IsNullOrWhiteSpace(w2) && !string.IsNullOrWhiteSpace(w3))
                        {
                            listWhere.Add(new List<string> { w1, w2, w3 });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                listWhere = null;
            }

            var vm = LoggingTo.Query(now.AddYears(-5), now, page, rows, listWhere);

            return new
            {
                data = vm.Data,
                total = vm.Total,
                lost = vm.Lost
            }.ToJson();
        }

        /// <summary>
        /// 日志图表
        /// </summary>
        /// <returns></returns>
        public IActionResult LogChart()
        {
            return View();
        }

        /// <summary>
        /// 查询日志流量
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="LogGroup">分组</param>
        /// <returns></returns>
        [ResponseCache(Duration = 10)]
        public LoggingResultVM QueryLogStatsPVUV(int? type, string LogGroup)
        {
            var listWhere = new List<List<string>>();
            if (!string.IsNullOrWhiteSpace(LogGroup))
            {
                listWhere = new List<List<string>>
                {
                    new List<string> { "LogGroup", "=", LogGroup }
                };
            }

            var vm = LoggingTo.StatsPVUV(type ?? 0, listWhere);
            return vm;
        }

        /// <summary>
        /// 查询日志Top
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="field">属性字段</param>
        /// <param name="LogGroup">分组</param>
        /// <returns></returns>
        [ResponseCache(Duration = 10)]
        public LoggingResultVM QueryLogReportTop(int? type, string field, string LogGroup)
        {
            var listWhere = new List<List<string>>();
            if (!string.IsNullOrWhiteSpace(LogGroup))
            {
                listWhere = new List<List<string>>
                {
                    new List<string> { "LogGroup", "=", LogGroup }
                };
            }

            var vm = LoggingTo.StatsTop(type ?? 0, field, listWhere);
            return vm;
        }

        #endregion

        #region 百科字典

        /// <summary>
        /// 字典
        /// </summary>
        /// <returns></returns>
        public IActionResult KeyValues()
        {
            string cmd = RouteData.Values["id"]?.ToString();
            if (cmd != null)
            {
                string result = string.Empty;
                var rt = new List<object>
                {
                    0,
                    "fail"
                };

                try
                {
                    switch (cmd)
                    {
                        case "grab":
                            {
                                string key = Request.Form["Key"].ToString();
                                string api = $"https://baike.baidu.com/api/openapi/BaikeLemmaCardApi?scope=103&format=json&appid=379020&bk_key={key.ToUrlEncode()}&bk_length=600";
                                string apirt = Core.HttpTo.Get(api);
                                if (apirt.Length > 100)
                                {
                                    var kvMo = db.KeyValues.FirstOrDefault(x => x.KeyName == key);
                                    if (kvMo == null)
                                    {
                                        kvMo = new Domain.KeyValues
                                        {
                                            KeyId = Guid.NewGuid().ToString(),
                                            KeyName = key.ToLower(),
                                            KeyValue = apirt
                                        };
                                        db.KeyValues.Add(kvMo);
                                    }
                                    else
                                    {
                                        kvMo.KeyValue = apirt;
                                        db.KeyValues.Update(kvMo);
                                    }

                                    rt[0] = db.SaveChanges();
                                    rt[1] = kvMo;
                                }
                                else
                                {
                                    rt[0] = 0;
                                    rt[1] = apirt;
                                }
                            }
                            break;
                        case "synonym":
                            {
                                var keys = Request.Form["keys"].ToString().Split(',').ToList();

                                string mainKey = keys.First().ToLower();
                                keys.RemoveAt(0);

                                var listkvs = new List<Domain.KeyValueSynonym>();
                                foreach (var key in keys)
                                {
                                    var kvs = new Domain.KeyValueSynonym
                                    {
                                        KsId = Guid.NewGuid().ToString(),
                                        KeyName = mainKey,
                                        KsName = key.ToLower()
                                    };
                                    listkvs.Add(kvs);
                                }

                                var mo = db.KeyValueSynonym.FirstOrDefault(x => x.KeyName == mainKey);
                                if (mo != null)
                                {
                                    db.KeyValueSynonym.Remove(mo);
                                }
                                db.KeyValueSynonym.AddRange(listkvs);
                                int oldrow = db.SaveChanges();
                                rt[0] = 1;
                                rt[1] = " 受影响 " + oldrow + " 行";
                            }
                            break;
                        case "addtag":
                            {
                                var tags = Request.Form["tags"].ToString().ToLower().Split(',').ToList();

                                if (tags.Count > 0)
                                {
                                    var mt = db.Tags.Where(x => tags.Contains(x.TagName)).ToList();
                                    if (mt.Count == 0)
                                    {
                                        var listMo = new List<Domain.Tags>();
                                        var tagHs = new HashSet<string>();
                                        foreach (var tag in tags)
                                        {
                                            if (tagHs.Add(tag))
                                            {
                                                var mo = new Domain.Tags
                                                {
                                                    TagName = tag,
                                                    TagCode = tag,
                                                    TagStatus = 1,
                                                    TagHot = 0,
                                                    TagIcon = tag + ".svg"
                                                };
                                                listMo.Add(mo);
                                            }
                                        }
                                        tagHs.Clear();

                                        //新增&刷新缓存
                                        db.Tags.AddRange(listMo);
                                        rt[0] = db.SaveChanges();

                                        Application.CommonService.TagsQuery(false);

                                        rt[1] = "操作成功（已刷新缓存）";
                                    }
                                    else
                                    {
                                        rt[0] = 0;
                                        rt[1] = "标签已存在：" + mt.ToJson();
                                    }
                                }
                                else
                                {
                                    Application.CommonService.TagsQuery(false);

                                    rt[0] = 0;
                                    rt[1] = "新增标签不能为空（已刷新缓存）";
                                }
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    rt[1] = ex.Message;
                    rt.Add(ex.StackTrace);
                }

                result = rt.ToJson();
                return Content(result);
            }
            return View();
        }

        #endregion
    }
}