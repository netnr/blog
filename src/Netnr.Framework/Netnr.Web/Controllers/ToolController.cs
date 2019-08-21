using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Netnr.Data;
using System.ComponentModel;

namespace Netnr.Web.Controllers
{
    /// <summary>
    /// 工具
    /// </summary>
    public class ToolController : Controller
    {
        [Description("工具页面")]
        public IActionResult Index()
        {
            return Redirect("https://ss.netnr.com");
        }

        #region 记事
        [Description("记事页面")]
        [Authorize]
        public IActionResult Note()
        {
            return View();
        }

        [Description("记事列表")]
        [Authorize]
        public string QueryNote(string KeyWords)
        {
            KeyWords = KeyWords ?? "";
            string result = "[]";
            int uid = new Func.UserAuthAid(HttpContext).Get().UserId;
            using (var db = new ContextBase())
            {
                var list = from x in db.Notepad
                           where x.Uid == uid && (x.NoteTitle.Contains(KeyWords))
                           orderby x.NoteCreateTime descending
                           select new
                           {
                               x.NoteId,
                               x.NoteTitle,
                               x.NoteCreateTime,
                               x.NoteUpdateTime
                           };
                result = list.ToList().ToJson();
            }

            return result;
        }

        [Description("获取一条记事")]
        [Authorize]
        public string QueryNoteOne(int NotepadId)
        {
            using (var db = new ContextBase())
            {
                var mo = db.Notepad.Find(NotepadId);
                return mo.ToJson();
            }
        }

        [Description("删除记事")]
        [Authorize]
        public int DelNote(int NotepadId)
        {
            int num = 0;
            using (var db = new ContextBase())
            {
                var mo = db.Notepad.Find(NotepadId);
                if (mo != null)
                {
                    db.Notepad.Remove(mo);
                    num = db.SaveChanges();
                }
            }
            return num;
        }

        [Description("保存记事")]
        [Authorize]
        public Domain.Notepad SaveNote(Domain.Notepad mo)
        {
            mo.Uid = new Func.UserAuthAid(HttpContext).Get().UserId;

            if (string.IsNullOrEmpty(mo.NoteTitle))
            {
                mo.NoteTitle = mo.NoteContent.Length > 20 ? mo.NoteContent.Substring(0, 20) : mo.NoteContent;
            }
            mo.NoteUpdateTime = DateTime.Now;

            using (var db = new ContextBase())
            {
                if (mo.NoteId == 0)
                {
                    mo.NoteCreateTime = mo.NoteUpdateTime;
                    db.Notepad.Add(mo);
                }
                else
                {
                    db.Notepad.Update(mo);
                }
                db.SaveChanges();
            }

            return mo;
        }
        #endregion
    }
}