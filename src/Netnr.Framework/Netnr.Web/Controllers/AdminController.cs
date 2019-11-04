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
    [Authorize]
    [FilterConfigs.IsAdmin]
    public class AdminController : Controller
    {
        [Description("后台管理")]
        public IActionResult Index()
        {
            return View();
        }

        [Description("查询文章列表")]
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

        [Description("保存一篇文章（管理）")]
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

    }
}