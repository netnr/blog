using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Netnr.Data;
using Netnr.Func.ViewModel;

namespace Netnr.Web.Controllers
{
    public class NoteController : Controller
    {
        [Description("记事本")]
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Description("查询记事本列表")]
        [Authorize]
        public QueryDataOutputVM QueryNoteList(QueryDataInputVM ivm)
        {
            var ovm = new QueryDataOutputVM();

            var uinfo = new Func.UserAuthAid(HttpContext).Get();

            using (var db = new ContextBase())
            {
                var query = from a in db.Notepad
                            join b in db.UserInfo on a.Uid equals b.UserId
                            orderby a.NoteCreateTime descending
                            where a.Uid == uinfo.UserId
                            select new Domain.Notepad
                            {
                                NoteId = a.NoteId,
                                NoteTitle = a.NoteTitle,
                                NoteCreateTime = a.NoteCreateTime,
                                NoteUpdateTime = a.NoteUpdateTime,
                                Uid = a.Uid,

                                Spare3 = b.Nickname
                            };

                if (!string.IsNullOrWhiteSpace(ivm.pe1))
                {
                    query = query.Where(x => x.NoteTitle.Contains(ivm.pe1));
                }

                Func.Common.QueryJoin(query, ivm, ref ovm);
            }
            return ovm;
        }

        [Description("保存一条记事本")]
        [Authorize]
        public ActionResultVM SaveNote(Domain.Notepad mo)
        {
            var vm = new ActionResultVM();

            var uinfo = new Func.UserAuthAid(HttpContext).Get();

            using (var db = new ContextBase())
            {
                var now = DateTime.Now;
                if (mo.NoteId == 0)
                {
                    mo.NoteCreateTime = now;
                    mo.NoteUpdateTime = now;
                    mo.Uid = uinfo.UserId;

                    db.Notepad.Add(mo);

                    int num = db.SaveChanges();
                    vm.Set(num > 0);
                    vm.data = mo.NoteId;
                }
                else
                {
                    var currmo = db.Notepad.Find(mo.NoteId);
                    if (currmo.Uid == uinfo.UserId)
                    {
                        currmo.NoteTitle = mo.NoteTitle;
                        currmo.NoteContent = mo.NoteContent;
                        currmo.NoteUpdateTime = now;

                        db.Notepad.Update(currmo);

                        int num = db.SaveChanges();

                        vm.Set(num > 0);
                    }
                    else
                    {
                        vm.Set(ARTag.unauthorized);
                    }
                }
            }

            return vm;
        }

        [Description("查询一条记事本详情")]
        [Authorize]
        public ActionResultVM QueryNoteOne(int id)
        {
            var vm = new ActionResultVM();

            var uinfo = new Func.UserAuthAid(HttpContext).Get();

            using (var db = new ContextBase())
            {
                var mo = db.Notepad.Find(id);
                if (mo == null)
                {
                    vm.Set(ARTag.invalid);
                }
                else if (mo.Uid == uinfo.UserId)
                {
                    vm.Set(ARTag.success);
                    vm.data = mo;
                }
                else
                {
                    vm.Set(ARTag.unauthorized);
                }
            }

            return vm;
        }

        [Description("删除一条记事本")]
        [Authorize]
        public ActionResultVM DelNote(int id)
        {
            var vm = new ActionResultVM();

            var uinfo = new Func.UserAuthAid(HttpContext).Get();

            using (var db = new ContextBase())
            {
                var mo = db.Notepad.Find(id);
                if (mo.Uid == uinfo.UserId)
                {
                    db.Notepad.Remove(mo);
                    int num = db.SaveChanges();

                    vm.Set(num > 0);
                }
                else
                {
                    vm.Set(ARTag.unauthorized);
                }
            }

            return vm;
        }
    }
}