using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Netnr.Data;
using Netnr.Func.ViewModel;
using Netnr.Web.Filters;

namespace Netnr.Web.Controllers
{
    /// <summary>
    /// 后台管理
    /// </summary>
    [Authorize]
    [FilterConfigs.IsAdmin]
    public class AdminController : Controller
    {
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

            using (var db = new ContextBase())
            {
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

                if (!string.IsNullOrWhiteSpace(ivm.pe1))
                {
                    query = query.Where(x => x.UwTitle.Contains(ivm.pe1));
                }

                Func.Common.QueryJoin(query, ivm, ref ovm);
            }

            return ovm;
        }

        /// <summary>
        /// 保存一篇文章（管理）
        /// </summary>
        /// <param name="mo"></param>
        /// <returns></returns>
        public ActionResultVM WriteAdminSave(Domain.UserWriting mo)
        {
            var vm = new ActionResultVM();

            var uinfo = new Func.UserAuthAid(HttpContext).Get();

            using (var db = new ContextBase())
            {
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

            using (var db = new ContextBase())
            {
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

                                b.UserId,
                                b.Nickname,
                                b.UserName,
                                b.UserMail
                            };

                if (!string.IsNullOrWhiteSpace(ivm.pe1))
                {
                    query = query.Where(x => x.UrContent.Contains(ivm.pe1));
                }

                Func.Common.QueryJoin(query, ivm, ref ovm);
            }

            return ovm;
        }

        /// <summary>
        /// 保存一条回复（管理）
        /// </summary>
        /// <param name="mo"></param>
        /// <returns></returns>
        public ActionResultVM ReplyAdminSave(Domain.UserReply mo)
        {
            var vm = new ActionResultVM();

            var uinfo = new Func.UserAuthAid(HttpContext).Get();

            using (var db = new ContextBase())
            {
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
        /// <returns></returns>
        [ResponseCache(Duration = 10)]
        public string QueryLog(int page, int rows)
        {
            return Func.LogsAid.Query(page, rows);
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
        /// <returns></returns>
        [ResponseCache(Duration = 10)]
        public string QueryLogReportFlow(int? type)
        {
            return Func.LogsAid.ReportFlow(type ?? 0);
        }

        /// <summary>
        /// 查询日志Top
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="field">属性字段</param>
        /// <returns></returns>
        [ResponseCache(Duration = 10)]
        public string QueryLogReportTop(int? type, string field)
        {
            return Func.LogsAid.ReportTop(type ?? 0, field);
        }

        #endregion
    }
}