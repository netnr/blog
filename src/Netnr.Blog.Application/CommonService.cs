using Microsoft.EntityFrameworkCore;
using JiebaNet.Segmenter;
using LinqKit;
using Netnr.Core;
using Netnr.Blog.Data;
using Netnr.Blog.Domain;
using Netnr.SharedFast;

namespace Netnr.Blog.Application
{
    /// <summary>
    /// 公共常用
    /// </summary>
    public class CommonService
    {
        /// <summary>
        /// 查询拼接
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="ivm"></param>
        /// <param name="ovm"></param>
        public static void QueryJoin<T>(IQueryable<T> query, QueryDataInputVM ivm, ref QueryDataOutputVM ovm)
        {
            //总条数
            ovm.Total = query.Count();

            //排序
            if (!string.IsNullOrWhiteSpace(ivm.Sort))
            {
                query = QueryableTo.OrderBy(query, ivm.Sort, ivm.Order);
            }

            //分页
            if (ivm.Pagination == 1)
            {
                query = query.Skip((ivm.Page - 1) * ivm.Rows).Take(ivm.Rows);
            }

            var sql = query.ToQueryString();
            Console.WriteLine(sql);

            //数据
            var data = query.ToList();
            ovm.Data = data;
        }

        /// <summary>
        /// 获取所有标签
        /// </summary>
        /// <param name="FirtCache">默认取缓存</param>
        /// <returns></returns>
        public static List<Tags> TagsQuery(bool FirtCache = true)
        {
            if (CacheTo.Get("Table_Tags_List") is not List<Tags> lt || !FirtCache)
            {
                using var db = ContextBaseFactory.CreateDbContext();
                lt = db.Tags.Where(x => x.TagStatus == 1).OrderByDescending(x => x.TagHot).ToList();
                CacheTo.Set("Table_Tags_List", lt, 300, false);
            }
            return lt;
        }

        /// <summary>
        /// 获取文章标签统计
        /// </summary>
        /// <param name="FirtCache"></param>
        /// <returns></returns>
        public static Dictionary<string, int> UserWritingByTagCountQuery(bool FirtCache = true)
        {
            if ((CacheTo.Get("Table_WritingTags_GroupBy") is not Dictionary<string, int> rt) || !FirtCache)
            {
                using var db = ContextBaseFactory.CreateDbContext();
                var query = from a in db.UserWritingTags
                            group a by a.TagName into m
                            orderby m.Count() descending
                            select new
                            {
                                m.Key,
                                total = m.Count()
                            };
                var qs = query.Take(20).OrderByDescending(x => x.total).ToList();

                var dic = new Dictionary<string, int>();
                foreach (var item in qs)
                {
                    dic.Add(item.Key, item.total);
                }

                rt = dic;
                CacheTo.Set("Table_WritingTags_GroupBy", rt, 300, false);
            }
            return rt;
        }

        /// <summary>
        /// 获取文章列表
        /// </summary>
        /// <param name="KeyWords"></param>
        /// <param name="page"></param>
        /// <param name="TagName"></param>
        /// <returns></returns>
        public static SharedPageVM UserWritingQuery(string KeyWords, int page, string TagName = "")
        {
            var vm = new SharedPageVM();

            KeyWords ??= "";

            var pag = new SharedPaginationVM
            {
                PageNumber = Math.Max(page, 1),
                PageSize = 12
            };

            var dicQs = new Dictionary<string, string> { { "k", KeyWords } };

            IQueryable<UserWriting> query;

            using var db = ContextBaseFactory.CreateDbContext();
            if (!string.IsNullOrWhiteSpace(TagName))
            {
                query = from a in db.UserWritingTags.Where(x => x.TagName == TagName)
                        join b in db.UserWriting on a.UwId equals b.UwId
                        select b;
                query = query.Distinct();
            }
            else
            {
                query = from a in db.UserWriting select a;
            }

            query = query.Where(x => x.UwOpen == 1 && x.UwStatus == 1);

            if (!string.IsNullOrWhiteSpace(KeyWords))
            {
                var kws = new JiebaSegmenter().Cut(KeyWords).ToList();
                kws.Add(KeyWords);
                kws = kws.Distinct().ToList();

                var inner = PredicateBuilder.New<UserWriting>();
                switch (GlobalTo.TDB)
                {
                    case SharedEnum.TypeDB.SQLite:
                        kws.ForEach(k => inner.Or(x => EF.Functions.Like(x.UwTitle, $"%{k}%")));
                        break;
                    case SharedEnum.TypeDB.PostgreSQL:
                        kws.ForEach(k => inner.Or(x => EF.Functions.ILike(x.UwTitle, $"%{k}%")));
                        break;
                    default:
                        kws.ForEach(k => inner.Or(x => x.UwTitle.Contains(k)));
                        break;
                }

                query = query.Where(inner);
            }

            pag.Total = query.Count();

            query = query.OrderByDescending(x => x.UwId).Skip((pag.PageNumber - 1) * pag.PageSize).Take(pag.PageSize);

            var list = query.ToList();

            //文章ID
            var listUwId = list.Select(x => x.UwId).ToList();

            //文章的所有的标签
            var queryTags = from a in db.UserWritingTags
                            join b in db.Tags on a.TagName equals b.TagName
                            where listUwId.Contains(a.UwId) || b.TagName == TagName
                            orderby a.UwtId ascending
                            select new
                            {
                                a.UwId,
                                a.TagName,
                                b.TagIcon
                            };
            var listUwTags = queryTags.ToList();

            //文章人员ID
            var listUwUid = list.Select(x => x.UwLastUid).Concat(list.Select(x => x.Uid)).Distinct();

            //文章人员ID对应的信息
            var listUwUserInfo = db.UserInfo.Where(x => listUwUid.Contains(x.UserId)).Select(x => new { x.UserId, x.Nickname }).ToList();

            //把信息赋值到文章表的备用字段上
            foreach (var item in list)
            {
                //标签
                item.Spare1 = listUwTags.Where(x => x.UwId == item.UwId).Select(x => new { x.TagName, x.TagIcon }).ToJson();

                //写主昵称
                item.Spare3 = listUwUserInfo.FirstOrDefault(x => x.UserId == item.Uid)?.Nickname;

                //有回复
                if (item.UwLastUid > 0)
                {
                    //回复用户昵称
                    item.Spare2 = listUwUserInfo.FirstOrDefault(x => x.UserId == item.UwLastUid)?.Nickname;
                }
            }

            vm.Rows = list;
            vm.Pag = pag;
            vm.QueryString = dicQs;

            if (!string.IsNullOrWhiteSpace(TagName))
            {
                try
                {
                    var jt = KeyValuesQuery(new List<string> { TagName }).FirstOrDefault()?.KeyValue.ToJObject();
                    if (jt != null)
                    {
                        var tags = new List<object>
                        {
                            new
                            {
                                TagName,
                                listUwTags.FirstOrDefault(x => x.TagName == TagName)?.TagIcon
                            }
                        };

                        vm.Temp = new
                        {
                            abs = new List<string>
                            {
                                jt["abstract"].ToString(),
                                jt["url"].ToString()
                            },
                            tags
                        }.ToJson();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            return vm;
        }

        /// <summary>
        /// 获取关联的文章列表
        /// </summary>
        /// <param name="OwnerId">所属用户关联</param>
        /// <param name="connectionType">关联分类</param>
        /// <param name="action">动作</param>
        /// <param name="page"></param>
        /// <returns></returns>
        public static SharedPageVM UserConnWritingQuery(int OwnerId, EnumService.ConnectionType connectionType, int action, int page)
        {
            var pag = new SharedPaginationVM
            {
                PageNumber = Math.Max(page, 1),
                PageSize = 20
            };

            using var db = ContextBaseFactory.CreateDbContext();
            IQueryable<UserWriting> query = null;

            switch (connectionType)
            {
                case EnumService.ConnectionType.UserWriting:
                    {
                        query = from a in db.UserConnection
                                join b in db.UserWriting on a.UconnTargetId equals b.UwId.ToString()
                                where a.Uid == OwnerId && a.UconnTargetType == connectionType.ToString() && a.UconnAction == action
                                orderby a.UconnCreateTime descending
                                select b;
                    }
                    break;
            }

            if (query == null)
            {
                return null;
            }

            pag.Total = query.Count();

            query = query.Skip((pag.PageNumber - 1) * pag.PageSize).Take(pag.PageSize);

            var list = query.ToList();

            //文章ID
            var listUwId = list.Select(x => x.UwId).ToList();

            //文章的所有的标签
            var listUwTags = (from a in db.Tags
                              join b in db.UserWritingTags on a.TagName equals b.TagName
                              where listUwId.Contains(b.UwId)
                              select new
                              {
                                  b.UwId,
                                  b.TagName,
                                  a.TagIcon
                              }).ToList();

            //文章人员ID
            var listUwUid = list.Select(x => x.UwLastUid).Concat(list.Select(x => x.Uid)).Distinct();

            //文章人员ID对应的信息
            var listUwUserInfo = db.UserInfo.Where(x => listUwUid.Contains(x.UserId)).Select(x => new { x.UserId, x.Nickname }).ToList();

            //把信息赋值到文章表的备用字段上
            foreach (var item in list)
            {
                //标签
                item.Spare1 = listUwTags.Where(x => x.UwId == item.UwId).Select(x => new { x.TagName, x.TagIcon }).ToJson();

                //写主昵称
                item.Spare3 = listUwUserInfo.FirstOrDefault(x => x.UserId == item.Uid)?.Nickname;

                //有回复
                if (item.UwLastUid > 0)
                {
                    //回复用户昵称
                    item.Spare2 = listUwUserInfo.FirstOrDefault(x => x.UserId == item.UwLastUid)?.Nickname;
                }
            }

            var vm = new SharedPageVM()
            {
                Rows = list,
                Pag = pag
            };

            return vm;
        }

        /// <summary>
        /// 获取一篇文章详情
        /// </summary>
        /// <param name="UwId"></param>
        /// <returns></returns>
        public static UserWriting UserWritingOneQuery(int UwId)
        {
            using var db = ContextBaseFactory.CreateDbContext();
            var one = db.UserWriting.Find(UwId);
            if (one == null)
            {
                return null;
            }

            //标签
            var onetags = (from a in db.UserWritingTags
                           join b in db.Tags on a.TagName equals b.TagName
                           where a.UwId == one.UwId
                           select new
                           {
                               a.TagName,
                               b.TagIcon
                           }).ToList();
            one.Spare1 = onetags.ToJson();

            //昵称
            var usermo = db.UserInfo.FirstOrDefault(x => x.UserId == one.Uid);
            one.Spare2 = usermo?.Nickname;

            return one;
        }

        /// <summary>
        /// 获取一个目标ID的回复
        /// </summary>
        /// <param name="replyType">回复分类</param>
        /// <param name="UrTargetId">回复目标ID</param>
        /// <param name="pag">分页信息</param>
        /// <returns></returns>
        public static List<UserReply> ReplyOneQuery(EnumService.ReplyType replyType, string UrTargetId, SharedPaginationVM pag)
        {
            using var db = ContextBaseFactory.CreateDbContext();
            var query = from a in db.UserReply
                        join b in db.UserInfo on a.Uid equals b.UserId into bg
                        from b1 in bg.DefaultIfEmpty()
                        where a.UrTargetType == replyType.ToString() && a.UrTargetId == UrTargetId
                        orderby a.UrCreateTime ascending
                        select new UserReply
                        {
                            Uid = a.Uid,
                            UrId = a.UrId,
                            UrStatus = a.UrStatus,
                            UrCreateTime = a.UrCreateTime,
                            UrContent = a.UrContent,
                            UrAnonymousLink = a.UrAnonymousLink,
                            UrAnonymousMail = a.UrAnonymousMail,
                            UrAnonymousName = a.UrAnonymousName,

                            UrTargetId = a.UrTargetId,

                            Spare1 = b1 == null ? null : b1.Nickname,
                            Spare2 = b1 == null ? null : b1.UserPhoto
                        };

            pag.Total = query.Count();

            query = query.Skip((pag.PageNumber - 1) * pag.PageSize).Take(pag.PageSize);

            var list = query.ToList();

            return list;
        }

        /// <summary>
        /// 键值对
        /// </summary>
        /// <param name="ListKeyName"></param>
        /// <returns></returns>
        public static List<KeyValues> KeyValuesQuery(List<string> ListKeyName)
        {
            using var db = ContextBaseFactory.CreateDbContext();
            var listKv = db.KeyValues.Where(x => ListKeyName.Contains(x.KeyName)).ToList();
            if (listKv.Count != ListKeyName.Count)
            {
                var hasK = listKv.Select(x => x.KeyName).ToList();
                var noK = new List<string>();
                foreach (var item in ListKeyName)
                {
                    if (!hasK.Contains(item))
                    {
                        noK.Add(item);
                    }
                }
                if (noK.Count > 0)
                {
                    var listKvs = db.KeyValueSynonym.Where(x => noK.Contains(x.KsName)).ToList();
                    if (listKvs.Count > 0)
                    {
                        var appendKey = listKvs.Select(x => x.KeyName).ToList();
                        var appendKv = db.KeyValues.Where(x => appendKey.Contains(x.KeyName)).ToList();
                        foreach (var item in appendKv)
                        {
                            var mc = listKvs.Where(x => x.KeyName == item.KeyName).FirstOrDefault();
                            if (mc != null)
                            {
                                item.KeyName = mc.KsName;
                            }
                        }
                        listKv.AddRange(appendKv);
                    }
                }
            }

            return listKv;
        }

        /// <summary>
        /// 获取消息
        /// </summary>
        /// <param name="UserId">用户ID</param>
        /// <param name="messageType">消息分类</param>
        /// <param name="action">消息动作</param>
        /// <param name="page"></param>
        /// <returns></returns>
        public static SharedPageVM MessageQuery(int UserId, EnumService.MessageType? messageType, int? action, int page = 1)
        {
            using var db = ContextBaseFactory.CreateDbContext();
            var query = from a in db.UserMessage
                        join b in db.UserInfo on a.UmTriggerUid equals b.UserId into bg
                        from b1 in bg.DefaultIfEmpty()
                        orderby a.UmCreateTime descending
                        where a.Uid == UserId
                        select new
                        {
                            a,
                            Nickname = b1 == null ? null : b1.Nickname,
                            UserPhoto = b1 == null ? null : b1.UserPhoto
                        };
            if (messageType.HasValue)
            {
                query = query.Where(x => x.a.UmType == messageType.ToString());
            }
            if (action.HasValue)
            {
                query = query.Where(x => x.a.UmAction == action);
            }

            var pag = new SharedPaginationVM
            {
                PageNumber = Math.Max(page, 1),
                PageSize = 10
            };

            pag.Total = query.Count();
            var list = query.Skip((pag.PageNumber - 1) * pag.PageSize).Take(pag.PageSize).ToList();

            if (list.Count > 0)
            {
                //分类：根据ID查询对应的标题
                var listUwId = list.Where(x => x.a.UmType == EnumService.MessageType.UserWriting.ToString()).Select(x => Convert.ToInt32(x.a.UmTargetId)).ToList();
                var listUw = db.UserWriting.Where(x => listUwId.Contains(x.UwId)).Select(x => new { x.UwId, x.UwTitle }).ToList();

                foreach (var item in list)
                {
                    item.a.Spare1 = item.Nickname;
                    item.a.Spare2 = item.UserPhoto;
                    item.a.Spare3 = listUw.FirstOrDefault(x => x.UwId.ToString() == item.a.UmTargetId)?.UwTitle;
                }
            }

            var data = list.Select(x => x.a).ToList();

            SharedPageVM pageSet = new()
            {
                Rows = data,
                Pag = pag
            };

            return pageSet;
        }

        /// <summary>
        /// 有新消息数量查询
        /// </summary>
        /// <param name="UserId">用户ID</param>
        /// <returns></returns>
        public static int NewMessageQuery(int UserId)
        {
            if (SharedFast.GlobalTo.GetValue<bool>("ReadOnly"))
            {
                return 0;
            }

            var ck = $"mq_{UserId}";
            var num = CacheTo.Get(ck) as int?;
            if (num == null)
            {
                using var db = ContextBaseFactory.CreateDbContext();
                num = db.UserMessage.Where(x => x.Uid == UserId && x.UmStatus == 1).Count();
                CacheTo.Set(ck, num, 10, false);
            }

            return num.Value;
        }

        /// <summary>
        /// Gist查询，按列权重排序
        /// </summary>
        /// <param name="q">搜索</param>
        /// <param name="lang">语言</param>
        /// <param name="OwnerId">所属用户</param>
        /// <param name="UserId">登录用户</param>
        /// <param name="page">页码</param>
        /// <returns></returns>
        public static SharedPageVM GistQuery(string q, string lang, int OwnerId = 0, int UserId = 0, int page = 1)
        {
            using var db = ContextBaseFactory.CreateDbContext();
            var query1 = from a in db.Gist
                         join b in db.UserInfo on a.Uid equals b.UserId
                         where a.GistStatus == 1
                         orderby a.GistCreateTime descending
                         select new
                         {
                             a,
                             b.Nickname
                         };

            if (!string.IsNullOrWhiteSpace(lang))
            {
                query1 = query1.Where(x => x.a.GistLanguage == lang);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                query1 = GlobalTo.TDB switch
                {
                    SharedEnum.TypeDB.SQLite => query1.Where(x => EF.Functions.Like(x.a.GistFilename, $"%{q}%") || EF.Functions.Like(x.a.GistRemark, $"%{q}%") || EF.Functions.Like(x.a.GistContent, $"%{q}%")),
                    SharedEnum.TypeDB.PostgreSQL => query1.Where(x => EF.Functions.ILike(x.a.GistFilename, $"%{q}%") || EF.Functions.ILike(x.a.GistRemark, $"%{q}%") || EF.Functions.ILike(x.a.GistContent, $"%{q}%")),
                    _ => query1.Where(x => x.a.GistFilename.Contains(q) || x.a.GistRemark.Contains(q) || x.a.GistContent.Contains(q)),
                };
            }

            //所属用户
            if (OwnerId != 0)
            {
                query1 = query1.Where(x => x.a.Uid == OwnerId);
            }

            //未登录
            if (UserId == 0)
            {
                query1 = query1.Where(x => x.a.GistOpen == 1);
            }
            else
            {
                //已登录：公开&登录用户的所有
                query1 = query1.Where(x => x.a.GistOpen == 1 || x.a.Uid == UserId);
            }

            IQueryable<Gist> query = null;

            //搜索
            if (!string.IsNullOrWhiteSpace(q))
            {
                var query2 = query1.Select(x => new
                {
                    SearchOrder = (x.a.GistFilename.Contains(q) ? 4 : 0) + (x.a.GistRemark.Contains(q) ? 2 : 0) + (x.a.GistContent.Contains(q) ? 1 : 0),
                    x.Nickname,
                    x.a
                }).OrderByDescending(x => x.SearchOrder);

                switch (GlobalTo.TDB)
                {
                    case SharedEnum.TypeDB.SQLite:
                        query2 = query1.Select(x => new
                        {
                            SearchOrder = (EF.Functions.Like(x.a.GistFilename, $"%{q}%") ? 4 : 0) + (EF.Functions.Like(x.a.GistRemark, $"%{q}%") ? 2 : 0) + (EF.Functions.Like(x.a.GistContent, $"%{q}%") ? 1 : 0),
                            x.Nickname,
                            x.a
                        }).OrderByDescending(x => x.SearchOrder);
                        break;
                    case SharedEnum.TypeDB.PostgreSQL:
                        query2 = query1.Select(x => new
                        {
                            SearchOrder = (EF.Functions.Like(x.a.GistFilename, $"%{q}%") ? 4 : 0) + (EF.Functions.Like(x.a.GistRemark, $"%{q}%") ? 2 : 0) + (EF.Functions.Like(x.a.GistContent, $"%{q}%") ? 1 : 0),
                            x.Nickname,
                            x.a
                        }).OrderByDescending(x => x.SearchOrder);
                        break;
                }

                query = query2.Select(x => new Gist
                {
                    GistCode = x.a.GistCode,
                    GistContentPreview = x.a.GistContentPreview,
                    GistCreateTime = x.a.GistCreateTime,
                    GistFilename = x.a.GistFilename,
                    GistId = x.a.GistId,
                    GistLanguage = x.a.GistLanguage,
                    GistRemark = x.a.GistRemark,
                    GistRow = x.a.GistRow,
                    GistTags = x.a.GistTags,
                    GistTheme = x.a.GistTheme,
                    Uid = x.a.Uid,

                    Spare3 = x.Nickname
                });
            }
            else
            {
                query = query1.Select(x => new Gist
                {
                    GistCode = x.a.GistCode,
                    GistContentPreview = x.a.GistContentPreview,
                    GistCreateTime = x.a.GistCreateTime,
                    GistFilename = x.a.GistFilename,
                    GistId = x.a.GistId,
                    GistLanguage = x.a.GistLanguage,
                    GistRemark = x.a.GistRemark,
                    GistRow = x.a.GistRow,
                    GistTags = x.a.GistTags,
                    GistTheme = x.a.GistTheme,
                    Uid = x.a.Uid,

                    Spare3 = x.Nickname
                });
            }

            var pag = new SharedPaginationVM
            {
                PageNumber = Math.Max(page, 1),
                PageSize = 10
            };

            var dicQs = new Dictionary<string, string> { { "q", q } };

            pag.Total = query.Count();
            var list = query.Skip((pag.PageNumber - 1) * pag.PageSize).Take(pag.PageSize).ToList();

            SharedPageVM pageSet = new()
            {
                Rows = list,
                Pag = pag,
                QueryString = dicQs
            };

            return pageSet;
        }

        /// <summary>
        /// Doc查询
        /// </summary>
        /// <param name="q">搜索</param>
        /// <param name="OwnerId">所属用户</param>
        /// <param name="UserId">登录用户</param>
        /// <param name="page">页码</param>
        /// <returns></returns>
        public static SharedPageVM DocQuery(string q, int OwnerId, int UserId, int page = 1)
        {
            using var db = ContextBaseFactory.CreateDbContext();
            var query = from a in db.DocSet
                        join b in db.UserInfo on a.Uid equals b.UserId
                        where a.DsStatus == 1
                        orderby a.DsCreateTime descending
                        select new DocSet
                        {
                            DsCode = a.DsCode,
                            DsCreateTime = a.DsCreateTime,
                            DsName = a.DsName,
                            DsOpen = a.DsOpen,
                            DsRemark = a.DsRemark,
                            Uid = a.Uid,
                            Spare1 = a.Spare1,

                            Spare3 = b.Nickname
                        };

            //所属用户
            if (OwnerId != 0)
            {
                query = query.Where(x => x.Uid == OwnerId);
            }

            //未登录
            if (UserId == 0)
            {
                query = query.Where(x => x.DsOpen == 1);
            }
            else
            {
                //已登录：公开&登录用户的所有
                query = query.Where(x => x.DsOpen == 1 || x.Uid == UserId);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = GlobalTo.TDB switch
                {
                    SharedEnum.TypeDB.SQLite => query.Where(x => EF.Functions.Like(x.DsName, $"%{q}%") || EF.Functions.Like(x.DsRemark, $"%{q}%")),
                    SharedEnum.TypeDB.PostgreSQL => query.Where(x => EF.Functions.ILike(x.DsName, $"%{q}%") || EF.Functions.ILike(x.DsRemark, $"%{q}%")),
                    _ => query.Where(x => x.DsName.Contains(q) || x.DsRemark.Contains(q)),
                };
            }

            var pag = new SharedPaginationVM
            {
                PageNumber = Math.Max(page, 1),
                PageSize = 20
            };

            var dicQs = new Dictionary<string, string> { { "q", q } };

            pag.Total = query.Count();
            var list = query.Skip((pag.PageNumber - 1) * pag.PageSize).Take(pag.PageSize).ToList();

            SharedPageVM pageSet = new()
            {
                Rows = list,
                Pag = pag,
                QueryString = dicQs
            };

            return pageSet;
        }

        /// <summary>
        /// Run查询
        /// </summary>
        /// <param name="q">搜索</param>
        /// <param name="OwnerId">所属用户</param>
        /// <param name="UserId">登录用户</param>
        /// <param name="page">页码</param>
        /// <returns></returns>
        public static SharedPageVM RunQuery(string q, int OwnerId = 0, int UserId = 0, int page = 1)
        {
            using var db = ContextBaseFactory.CreateDbContext();
            var query = from a in db.Run
                        join b in db.UserInfo on a.Uid equals b.UserId
                        where a.RunStatus == 1
                        orderby a.RunCreateTime descending
                        select new Run
                        {
                            RunCode = a.RunCode,
                            RunCreateTime = a.RunCreateTime,
                            RunId = a.RunId,
                            RunRemark = a.RunRemark,
                            RunTags = a.RunTags,
                            RunTheme = a.RunTheme,
                            Uid = a.Uid,
                            RunOpen = a.RunOpen,

                            Spare3 = b.Nickname,
                        };

            //所属用户
            if (OwnerId != 0)
            {
                query = query.Where(x => x.Uid == OwnerId);
            }

            //未登录
            if (UserId == 0)
            {
                query = query.Where(x => x.RunOpen == 1);
            }
            else
            {
                //已登录：公开&登录用户的所有
                query = query.Where(x => x.RunOpen == 1 || x.Uid == UserId);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = GlobalTo.TDB switch
                {
                    SharedEnum.TypeDB.SQLite => query.Where(x => EF.Functions.Like(x.RunRemark, $"%{q}%")),
                    SharedEnum.TypeDB.PostgreSQL => query.Where(x => EF.Functions.ILike(x.RunRemark, $"%{q}%")),
                    _ => query.Where(x => x.RunRemark.Contains(q)),
                };
            }

            var pag = new SharedPaginationVM
            {
                PageNumber = Math.Max(page, 1),
                PageSize = 24
            };

            var dicQs = new Dictionary<string, string> { { "q", q } };

            pag.Total = query.Count();
            var list = query.Skip((pag.PageNumber - 1) * pag.PageSize).Take(pag.PageSize).ToList();

            SharedPageVM pageSet = new()
            {
                Rows = list,
                Pag = pag,
                QueryString = dicQs
            };

            return pageSet;
        }

        /// <summary>
        /// Draw查询
        /// </summary>
        /// <param name="q">搜索</param>
        /// <param name="OwnerId">所属用户</param>
        /// <param name="UserId">登录用户</param>
        /// <param name="page">页码</param>
        /// <returns></returns>
        public static SharedPageVM DrawQuery(string q, int OwnerId = 0, int UserId = 0, int page = 1)
        {
            using var db = ContextBaseFactory.CreateDbContext();
            var query = from a in db.Draw
                        join b in db.UserInfo on a.Uid equals b.UserId
                        where a.DrStatus == 1
                        orderby a.DrCreateTime descending
                        select new Draw
                        {
                            DrId = a.DrId,
                            Uid = a.Uid,
                            DrType = a.DrType,
                            DrName = a.DrName,
                            DrRemark = a.DrRemark,
                            DrCategory = a.DrCategory,
                            DrOrder = a.DrOrder,
                            DrCreateTime = a.DrCreateTime,
                            DrStatus = a.DrStatus,
                            DrOpen = a.DrOpen,
                            Spare1 = a.Spare1,

                            Spare3 = b.Nickname
                        };

            //所属用户
            if (OwnerId != 0)
            {
                query = query.Where(x => x.Uid == OwnerId);
            }

            //未登录
            if (UserId == 0)
            {
                query = query.Where(x => x.DrOpen == 1);
            }
            else
            {
                //已登录：公开&登录用户的所有
                query = query.Where(x => x.DrOpen == 1 || x.Uid == UserId);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = GlobalTo.TDB switch
                {
                    SharedEnum.TypeDB.SQLite => query.Where(x => EF.Functions.Like(x.DrName, $"%{q}%") || EF.Functions.Like(x.DrRemark, $"%{q}%")),
                    SharedEnum.TypeDB.PostgreSQL => query.Where(x => EF.Functions.ILike(x.DrName, $"%{q}%") || EF.Functions.ILike(x.DrRemark, $"%{q}%")),
                    _ => query.Where(x => x.DrName.Contains(q) || x.DrRemark.Contains(q)),
                };
            }

            var pag = new SharedPaginationVM
            {
                PageNumber = Math.Max(page, 1),
                PageSize = 20
            };

            var dicQs = new Dictionary<string, string> { { "q", q } };

            pag.Total = query.Count();
            var list = query.Skip((pag.PageNumber - 1) * pag.PageSize).Take(pag.PageSize).ToList();

            SharedPageVM pageSet = new()
            {
                Rows = list,
                Pag = pag,
                QueryString = dicQs
            };

            return pageSet;
        }

        /// <summary>
        /// Guff查询
        /// </summary>
        /// <param name="category">类别，可选，支持 text、image、audio、video、me（我的）、melaud（我点赞的）、mereply（我回复的）</param>
        /// <param name="q">搜索</param>
        /// <param name="nv">分类名/分类值</param>
        /// <param name="tag">标签</param>
        /// <param name="obj">对象</param>
        /// <param name="OwnerId">所属用户</param>
        /// <param name="UserId">登录用户</param>
        /// <param name="page">页码</param>
        /// <returns></returns>
        public static SharedPageVM GuffQuery(string category, string q, string nv, string tag, string obj, int OwnerId, int UserId, int page = 1)
        {
            var ctype = EnumService.ConnectionType.GuffRecord.ToString();

            using var db = ContextBaseFactory.CreateDbContext();

            IQueryable<GuffRecord> query = null;

            switch (category?.ToLower())
            {
                case "melaud":
                    {
                        query = from c in db.UserConnection
                                join a in db.GuffRecord on c.UconnTargetId equals a.GrId
                                join b in db.UserInfo on a.Uid equals b.UserId
                                where c.Uid == UserId && c.UconnTargetType == ctype && c.UconnAction == 1 && a.GrStatus == 1
                                orderby c.UconnCreateTime descending
                                select new GuffRecord
                                {
                                    GrId = a.GrId,
                                    GrTypeName = a.GrTypeName,
                                    GrTypeValue = a.GrTypeValue,
                                    GrObject = a.GrObject,
                                    GrContent = a.GrContent,
                                    GrContentMd = a.GrContentMd,
                                    GrImage = a.GrImage,
                                    GrAudio = a.GrAudio,
                                    GrVideo = a.GrVideo,
                                    GrFile = a.GrFile,
                                    GrRemark = a.GrRemark,
                                    GrTag = a.GrTag,
                                    GrCreateTime = a.GrCreateTime,
                                    GrUpdateTime = a.GrUpdateTime,
                                    GrOpen = a.GrOpen,
                                    GrReadNum = a.GrReadNum,
                                    GrReplyNum = a.GrReplyNum,
                                    GrLaud = a.GrLaud,
                                    GrMark = a.GrMark,

                                    Uid = a.Uid,

                                    //已点赞
                                    Spare1 = "laud",
                                    //是我的
                                    Spare2 = a.Uid == UserId ? "owner" : "",
                                    //昵称
                                    Spare3 = b.Nickname
                                };
                    }
                    break;
                case "mereply":
                    {
                        query = from c in db.UserReply
                                join a in db.GuffRecord on c.UrTargetId equals a.GrId
                                join b in db.UserInfo on a.Uid equals b.UserId
                                where c.Uid == UserId && c.UrTargetType == ctype && a.GrStatus == 1
                                orderby c.UrCreateTime descending
                                select new GuffRecord
                                {
                                    GrId = a.GrId,
                                    GrTypeName = a.GrTypeName,
                                    GrTypeValue = a.GrTypeValue,
                                    GrObject = a.GrObject,
                                    GrContent = a.GrContent,
                                    GrContentMd = a.GrContentMd,
                                    GrImage = a.GrImage,
                                    GrAudio = a.GrAudio,
                                    GrVideo = a.GrVideo,
                                    GrFile = a.GrFile,
                                    GrRemark = a.GrRemark,
                                    GrTag = a.GrTag,
                                    GrCreateTime = a.GrCreateTime,
                                    GrUpdateTime = a.GrUpdateTime,
                                    GrOpen = a.GrOpen,
                                    GrReadNum = a.GrReadNum,
                                    GrReplyNum = a.GrReplyNum,
                                    GrLaud = a.GrLaud,
                                    GrMark = a.GrMark,

                                    Uid = a.Uid,

                                    Spare2 = a.Uid == UserId ? "owner" : "",
                                    Spare3 = b.Nickname
                                };
                    }
                    break;
                case "me":
                case "top":
                case "text":
                case "image":
                case "audio":
                case "video":
                default:
                    {
                        query = from a in db.GuffRecord
                                join b in db.UserInfo on a.Uid equals b.UserId
                                where a.GrStatus == 1
                                select new GuffRecord
                                {
                                    GrId = a.GrId,
                                    GrTypeName = a.GrTypeName,
                                    GrTypeValue = a.GrTypeValue,
                                    GrObject = a.GrObject,
                                    GrContent = a.GrContent,
                                    GrContentMd = a.GrContentMd,
                                    GrImage = a.GrImage,
                                    GrAudio = a.GrAudio,
                                    GrVideo = a.GrVideo,
                                    GrFile = a.GrFile,
                                    GrRemark = a.GrRemark,
                                    GrTag = a.GrTag,
                                    GrCreateTime = a.GrCreateTime,
                                    GrUpdateTime = a.GrUpdateTime,
                                    GrOpen = a.GrOpen,
                                    GrReadNum = a.GrReadNum,
                                    GrReplyNum = a.GrReplyNum,
                                    GrLaud = a.GrLaud,
                                    GrMark = a.GrMark,

                                    Uid = a.Uid,

                                    Spare2 = a.Uid == UserId ? "owner" : "",
                                    Spare3 = b.Nickname
                                };
                    }
                    break;
            }

            query = (category?.ToLower()) switch
            {
                "top" => query.OrderByDescending(x => x.GrLaud),
                "text" => query.OrderByDescending(x => x.GrCreateTime).Where(x => !string.IsNullOrEmpty(x.GrContent) && string.IsNullOrEmpty(x.GrImage) && string.IsNullOrEmpty(x.GrAudio) && string.IsNullOrEmpty(x.GrVideo)),
                "image" => query.OrderByDescending(x => x.GrCreateTime).Where(x => !string.IsNullOrEmpty(x.GrImage)),
                "audio" => query.OrderByDescending(x => x.GrCreateTime).Where(x => !string.IsNullOrEmpty(x.GrAudio)),
                "video" => query.OrderByDescending(x => x.GrCreateTime).Where(x => !string.IsNullOrEmpty(x.GrVideo)),
                _ => query.OrderByDescending(x => x.GrCreateTime),
            };

            //所属用户
            if (OwnerId != 0)
            {
                query = query.Where(x => x.Uid == OwnerId);
            }

            //未登录
            if (UserId == 0)
            {
                query = query.Where(x => x.GrOpen == 1);
            }
            else
            {
                //已登录：公开&登录用户的所有
                query = query.Where(x => x.GrOpen == 1 || x.Uid == UserId);
            }

            //分类名/分类值
            if (!string.IsNullOrWhiteSpace(nv))
            {
                if (!nv.Contains('/'))
                {
                    nv += "/";
                }

                var nvs = nv.Split('/').ToList();
                var n = nvs.FirstOrDefault();
                var v = nvs.LastOrDefault();

                //分类名
                if (!string.IsNullOrWhiteSpace(n))
                {
                    query = query.Where(x => x.GrTypeName == n);
                }

                //分类值
                if (!string.IsNullOrWhiteSpace(v))
                {
                    query = query.Where(x => x.GrTypeValue == v);
                }
            }

            //标签
            if (!string.IsNullOrWhiteSpace(tag))
            {
                query = query.Where(x => x.GrTag.Contains(tag));
            }

            //标签
            if (!string.IsNullOrWhiteSpace(tag))
            {
                query = query.Where(x => x.GrTag.Contains(tag));
            }

            //对象
            if (!string.IsNullOrWhiteSpace(obj))
            {
                query = query.Where(x => x.GrObject.Contains(obj));
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = GlobalTo.TDB switch
                {
                    SharedEnum.TypeDB.SQLite => query.Where(x => EF.Functions.Like(x.GrContent, $"%{q}%") || EF.Functions.Like(x.GrTag, $"%{q}%")),
                    SharedEnum.TypeDB.PostgreSQL => query.Where(x => EF.Functions.ILike(x.GrContent, $"%{q}%") || EF.Functions.ILike(x.GrTag, $"%{q}%")),
                    _ => query.Where(x => x.GrContent.Contains(q) || x.GrTag.Contains(q)),
                };
            }

            var pag = new SharedPaginationVM
            {
                PageNumber = Math.Max(page, 1),
                PageSize = 18
            };

            var dicQs = new Dictionary<string, string> { { "q", q } };

            pag.Total = query.Count();
            var list = query.Skip((pag.PageNumber - 1) * pag.PageSize).Take(pag.PageSize).ToList();

            var listid = list.Select(x => x.GrId).ToList();

            //点赞查询
            if (category != "melaud")
            {
                var listtid = db.UserConnection.Where(x => listid.Contains(x.UconnTargetId) && x.Uid == UserId && x.UconnTargetType == ctype && x.UconnAction == 1).Select(x => x.UconnTargetId).ToList();
                foreach (var item in list)
                {
                    if (listtid.Contains(item.GrId))
                    {
                        item.Spare1 = "laud";
                    }
                }
            }

            //查询记录
            var ormo = new OperationRecord()
            {
                OrId = UniqueTo.LongId().ToString(),
                OrType = ctype,
                OrAction = "query",
                OrSource = string.Join(",", listid),
                OrCreateTime = DateTime.Now,
                OrMark = "default"
            };
            db.OperationRecord.Add(ormo);
            db.SaveChanges();

            SharedPageVM pageSet = new()
            {
                Rows = list,
                Pag = pag,
                QueryString = dicQs
            };

            return pageSet;
        }

    }
}