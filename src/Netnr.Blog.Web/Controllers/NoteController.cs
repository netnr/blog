using Microsoft.AspNetCore.Authorization;
using Netnr.Blog.Data;

namespace Netnr.Blog.Web.Controllers
{
    /// <summary>
    /// 记事本
    /// </summary>
    public class NoteController : Controller
    {
        public ContextBase db;

        public NoteController(ContextBase cb)
        {
            db = cb;
        }

        /// <summary>
        /// 记事本
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 查询记事本列表
        /// </summary>
        /// <param name="ivm"></param>
        /// <returns></returns>
        [Authorize]
        public QueryDataOutputVM QueryNoteList(QueryDataInputVM ivm)
        {
            var ovm = new QueryDataOutputVM();

            var uinfo = Apps.LoginService.Get(HttpContext);

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

            if (!string.IsNullOrWhiteSpace(ivm.Pe1))
            {
                query = query.Where(x => x.NoteTitle.Contains(ivm.Pe1));
            }

            Application.CommonService.QueryJoin(query, ivm, ref ovm);

            return ovm;
        }

        /// <summary>
        /// 保存一条记事本
        /// </summary>
        /// <param name="mo"></param>
        /// <returns></returns>
        [Authorize]
        public SharedResultVM SaveNote(Domain.Notepad mo)
        {
            var vm = Apps.LoginService.CompleteInfoValid(HttpContext);
            if (vm.Code == 200)
            {
                if (string.IsNullOrWhiteSpace(mo.NoteTitle) || string.IsNullOrWhiteSpace(mo.NoteContent))
                {
                    vm.Set(SharedEnum.RTag.lack);
                }
                else
                {
                    var uinfo = Apps.LoginService.Get(HttpContext);

                    var now = DateTime.Now;
                    if (mo.NoteId == 0)
                    {
                        mo.NoteCreateTime = now;
                        mo.NoteUpdateTime = now;
                        mo.Uid = uinfo.UserId;

                        db.Notepad.Add(mo);

                        int num = db.SaveChanges();
                        vm.Set(num > 0);
                        vm.Data = mo.NoteId;
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
                            vm.Set(SharedEnum.RTag.unauthorized);
                        }
                    }
                }

            }

            return vm;
        }

        /// <summary>
        /// 查询一条记事本详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        public SharedResultVM QueryNoteOne(int id)
        {
            var vm = new SharedResultVM();

            var uinfo = Apps.LoginService.Get(HttpContext);

            var mo = db.Notepad.Find(id);
            if (mo == null)
            {
                vm.Set(SharedEnum.RTag.invalid);
            }
            else if (mo.Uid == uinfo.UserId)
            {
                vm.Set(SharedEnum.RTag.success);
                vm.Data = mo;
            }
            else
            {
                vm.Set(SharedEnum.RTag.unauthorized);
            }

            return vm;
        }

        /// <summary>
        /// 删除一条记事本
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        public SharedResultVM DelNote(int id)
        {
            var vm = new SharedResultVM();

            var uinfo = Apps.LoginService.Get(HttpContext);

            var mo = db.Notepad.Find(id);
            if (mo.Uid == uinfo.UserId)
            {
                db.Notepad.Remove(mo);
                int num = db.SaveChanges();

                vm.Set(num > 0);
            }
            else
            {
                vm.Set(SharedEnum.RTag.unauthorized);
            }

            return vm;
        }
    }
}