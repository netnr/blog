using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Netnr.Data;
using Netnr.Func.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Netnr.Web.Controllers
{
    /// <summary>
    /// 个人用户
    /// </summary>
    public class UserController : Controller
    {
        #region 消息

        /// <summary>
        /// 消息
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [Authorize]
        [ResponseCache(Duration = 5)]
        public IActionResult Message(int page = 1)
        {
            int uid = new Func.UserAuthAid(HttpContext).Get().UserId;

            var vm = Func.Common.MessageQuery(uid, Func.EnumAid.MessageType.UserWriting, null, page);
            vm.Route = Request.Path;

            if (page == 1)
            {
                using var db = new ContextBase();
                var listum = db.UserMessage.Where(x => x.UmType == Func.EnumAid.MessageType.UserWriting.ToString() && x.UmAction == 2 && x.UmStatus == 1).ToList();
                if (listum.Count > 0)
                {
                    listum.ForEach(x => x.UmStatus = 2);
                    db.UserMessage.UpdateRange(listum);
                    db.SaveChanges();
                }
            }

            return View(vm);
        }

        /// <summary>
        /// 删除消息
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public IActionResult DelMessage()
        {
            var vm = new ActionResultVM();

            var id = RouteData.Values["id"]?.ToString();
            if (!string.IsNullOrWhiteSpace(id))
            {
                var uinfo = new Func.UserAuthAid(HttpContext).Get();

                using var db = new ContextBase();
                var um = db.UserMessage.Find(id);
                if (um == null)
                {
                    vm.Set(ARTag.lack);
                }
                else if (um?.Uid != uinfo.UserId)
                {
                    vm.Set(ARTag.unauthorized);
                }
                else
                {
                    db.UserMessage.Remove(um);
                    int num = db.SaveChanges();

                    vm.Set(num > 0);
                }
            }

            if (vm.code == 200)
            {
                return Redirect("/user/message");
            }
            else
            {
                return Content(vm.ToJson());
            }
        }

        #endregion

        #region 主页

        /// <summary>
        /// 我的主页
        /// </summary>
        /// <returns></returns>
        public IActionResult Id()
        {
            if (int.TryParse(RouteData.Values["id"]?.ToString(), out int uid))
            {
                using var db = new ContextBase();
                var usermo = db.UserInfo.Find(uid);
                if (usermo != null)
                {
                    return View("_PartialU", usermo);
                }
            }

            return Content("Invalid");
        }

        /// <summary>
        /// 更新说说
        /// </summary>
        /// <param name="mo"></param>
        /// <returns></returns>
        [Authorize]
        public ActionResultVM UpdateUserSay(Domain.UserInfo mo)
        {
            var vm = new ActionResultVM();

            var uinfo = new Func.UserAuthAid(HttpContext).Get();
            using (var db = new ContextBase())
            {
                var currmo = db.UserInfo.Find(uinfo.UserId);
                currmo.UserSay = mo.UserSay;
                db.UserInfo.Update(currmo);

                int num = db.SaveChanges();

                vm.Set(num > 0);
            }

            return vm;
        }

        /// <summary>
        /// 更新头像
        /// </summary>
        /// <param name="type"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        [Authorize]
        public ActionResultVM UpdateUserPhoto(string type, string source)
        {
            var vm = new ActionResultVM();

            var uinfo = new Func.UserAuthAid(HttpContext).Get();

            try
            {
                var rootdir = GlobalTo.WebRootPath + "/" + (GlobalTo.GetValue("StaticResource:RootDir").TrimStart('/').TrimEnd('/') + "/");
                var path = GlobalTo.GetValue("StaticResource:AvatarPath").TrimEnd('/').TrimStart('/') + '/';
                var fullpath = rootdir + path;

                if (!Directory.Exists(fullpath))
                {
                    Directory.CreateDirectory(fullpath);
                }

                if (string.IsNullOrWhiteSpace(uinfo.UserPhoto))
                {
                    uinfo.UserPhoto = Core.UniqueTo.LongId() + ".jpg";
                }
                var upname = uinfo.UserPhoto.Split('?')[0];
                var npnew = upname + "?" + DateTime.Now.ToTimestamp();

                switch (type)
                {
                    case "file":
                        {
                            source = source.Substring(source.LastIndexOf(",") + 1);
                            byte[] bytes = Convert.FromBase64String(source);
                            using var ms = new MemoryStream(bytes);
                            using var bmp = new System.Drawing.Bitmap(ms);
                            var hp = fullpath + upname.Replace(".", "_lg.");
                            bmp.Save(hp, ImageFormat.Jpeg);
                            Fast.ImageTo.MinImg(hp, fullpath, upname, 40, 40, "wh");

                            using (var db = new ContextBase())
                            {
                                var usermo = db.UserInfo.Find(uinfo.UserId);
                                usermo.UserPhoto = npnew;
                                db.UserInfo.Update(usermo);
                                int num = db.SaveChanges();
                                if (num > 0)
                                {
                                    using var ac = new AccountController();
                                    ac.SetAuth(HttpContext, usermo);
                                }
                            }

                            vm.Set(ARTag.success);
                        }
                        break;
                    case "link":
                        {
                            using var wc = new System.Net.WebClient();
                            var hp = fullpath + upname.Replace(".", "_lg.");
                            wc.DownloadFile(source, hp);
                            Fast.ImageTo.MinImg(hp, fullpath, upname, 40, 40, "wh");

                            using (var db = new ContextBase())
                            {
                                var usermo = db.UserInfo.Find(uinfo.UserId);
                                usermo.UserPhoto = npnew;
                                db.UserInfo.Update(usermo);
                                int num = db.SaveChanges();
                                if (num > 0)
                                {
                                    using var ac = new AccountController();
                                    ac.SetAuth(HttpContext, usermo);
                                }
                            }

                            vm.Set(ARTag.success);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                vm.Set(ex);
            }

            return vm;
        }

        #endregion

        #region 个人设置

        /// <summary>
        /// 个人设置页面
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public IActionResult Setting()
        {
            var uinfo = new Func.UserAuthAid(HttpContext).Get();

            using (var db = new ContextBase())
            {
                var mo = db.UserInfo.Find(uinfo.UserId);
                return View(mo);
            };
        }

        /// <summary>
        /// 保存个人信息
        /// </summary>
        /// <param name="mo"></param>
        /// <returns></returns>
        [Authorize]
        public ActionResultVM SaveUserInfo(Domain.UserInfo mo)
        {
            var vm = new ActionResultVM();

            if (string.IsNullOrWhiteSpace(mo.Nickname))
            {
                vm.Set(ARTag.refuse);
                vm.msg = "昵称不能为空";

                return vm;
            }

            int uid = new Func.UserAuthAid(HttpContext).Get().UserId;
            using (var db = new ContextBase())
            {
                var usermo = db.UserInfo.Find(uid);

                //变更账号
                if (!string.IsNullOrWhiteSpace(mo.UserName) && usermo.UserNameChange != 1 && usermo.UserName != mo.UserName)
                {
                    //账号重复
                    if (db.UserInfo.Any(x => x.UserName == mo.UserName))
                    {
                        vm.Set(ARTag.exist);
                        vm.msg = "账号已经存在";

                        return vm;
                    }
                    else
                    {
                        usermo.UserName = mo.UserName;
                        usermo.UserNameChange = 1;
                    }
                }

                //变更邮箱
                if (mo.UserMail != usermo.UserMail)
                {
                    usermo.UserMailValid = 0;

                    //邮箱正则验证
                    if (!string.IsNullOrWhiteSpace(mo.UserMail))
                    {
                        if (!Fast.ParsingTo.IsMail(mo.UserMail))
                        {
                            vm.Set(ARTag.invalid);
                            vm.msg = "邮箱格式有误";

                            return vm;
                        }
                        else
                        {
                            if (db.UserInfo.Any(x => x.UserMail == mo.UserMail))
                            {
                                vm.Set(ARTag.exist);
                                vm.msg = "邮箱已经存在";

                                return vm;
                            }
                        }
                    }
                }

                usermo.UserMail = mo.UserMail;
                usermo.Nickname = mo.Nickname;
                usermo.UserPhone = mo.UserPhone;
                usermo.UserUrl = mo.UserUrl;

                db.UserInfo.Update(usermo);
                var num = db.SaveChanges();

                //更新授权信息
                using (var ac = new AccountController())
                {
                    ac.SetAuth(HttpContext, usermo, true);
                }

                vm.Set(num > 0);
            };

            return vm;
        }

        /// <summary>
        /// 绑定账号
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public IActionResult OAuth()
        {
            var authType = RouteData.Values["id"]?.ToString();
            var url = Func.UserAuthAid.ThirdLogin.LoginLink(authType, "bind");
            return Redirect(url);
        }

        /// <summary>
        /// 解绑账号
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public IActionResult RidOAuth()
        {
            if (Enum.TryParse(RouteData.Values["id"]?.ToString().ToLower(), out AccountController.ValidateloginType vtype))
            {
                int uid = new Func.UserAuthAid(HttpContext).Get().UserId;
                using var db = new ContextBase();
                var mo = db.UserInfo.Find(uid);

                switch (vtype)
                {
                    case AccountController.ValidateloginType.qq:
                        mo.OpenId1 = "";
                        break;
                    case AccountController.ValidateloginType.weibo:
                        mo.OpenId2 = "";
                        break;
                    case AccountController.ValidateloginType.github:
                        mo.OpenId3 = "";
                        break;
                    case AccountController.ValidateloginType.taobao:
                        mo.OpenId4 = "";
                        break;
                    case AccountController.ValidateloginType.microsoft:
                        mo.OpenId5 = "";
                        break;
                    case AccountController.ValidateloginType.dingtalk:
                        mo.OpenId6 = "";
                        break;
                }

                db.UserInfo.Update(mo);
                db.SaveChanges();
            }
            return Redirect("/user/setting");
        }

        /// <summary>
        /// 更新密码
        /// </summary>
        /// <param name="oldpwd"></param>
        /// <param name="newpwd"></param>
        /// <returns></returns>
        [Authorize]
        public ActionResultVM UpdatePassword(string oldpwd, string newpwd)
        {
            var vm = new ActionResultVM();

            int uid = new Func.UserAuthAid(HttpContext).Get().UserId;
            using (var db = new ContextBase())
            {
                var userinfo = db.UserInfo.Find(uid);
                if (userinfo.UserPwd == Core.CalcTo.MD5(oldpwd))
                {
                    userinfo.UserPwd = Core.CalcTo.MD5(newpwd);
                    db.UserInfo.Update(userinfo);
                    var num = db.SaveChanges();

                    vm.Set(num > 0);
                }
                else
                {
                    vm.Set(ARTag.unauthorized);
                }
            };

            return vm;
        }

        #endregion

        #region 文章管理

        /// <summary>
        /// 文章管理
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [Authorize]
        public IActionResult Write(int? page)
        {
            var id = RouteData.Values["id"]?.ToString();
            if (!string.IsNullOrWhiteSpace(id))
            {
                int action = 1;
                if (id == "mark")
                {
                    action = 2;
                }

                var uinfo = new Func.UserAuthAid(HttpContext).Get();
                var vm = Func.Common.UserConnWritingQuery(uinfo.UserId, Func.EnumAid.ConnectionType.UserWriting, action, page ?? 1);
                vm.Route = Request.Path;
                vm.Other = id;

                return View("_PartialViewWriting", vm);
            }

            return View();
        }

        /// <summary>
        /// 文章列表
        /// </summary>
        /// <param name="sort"></param>
        /// <param name="order"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="pe1"></param>
        /// <returns></returns>
        [Authorize]
        public string WriteList(string sort, string order, int page = 1, int rows = 30, string pe1 = null)
        {
            string result = string.Empty;

            var pag = new PaginationVM
            {
                PageNumber = page,
                PageSize = rows
            };

            int uid = new Func.UserAuthAid(HttpContext).Get().UserId;

            using var db = new ContextBase();
            var query = from a in db.UserWriting
                        where a.Uid == uid
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
                            a.UwCategory
                        };

            if (!string.IsNullOrWhiteSpace(pe1))
            {
                query = query.Where(x => x.UwTitle.Contains(pe1));
            }

            query = Fast.QueryableTo.OrderBy(query, sort, order);

            pag.Total = query.Count();
            var list = query.Skip((pag.PageNumber - 1) * pag.PageSize).Take(pag.PageSize).ToList();

            result = new
            {
                data = list,
                total = pag.Total
            }.ToJson();

            return result;
        }

        /// <summary>
        /// 获取一篇文章
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        public ActionResultVM WriteOne(int id)
        {
            var vm = new ActionResultVM();

            int uid = new Func.UserAuthAid(HttpContext).Get().UserId;
            using (var db = new ContextBase())
            {
                var mo = db.UserWriting.Where(x => x.Uid == uid && x.UwId == id).FirstOrDefault();
                var listTags = db.UserWritingTags.Where(x => x.UwId == id).ToList();

                vm.data = new
                {
                    item = mo,
                    tags = listTags
                };
                vm.Set(ARTag.success);
            }

            return vm;
        }

        /// <summary>
        /// 保存一篇文章（编辑）
        /// </summary>
        /// <param name="mo"></param>
        /// <param name="UwId"></param>
        /// <param name="TagIds"></param>
        /// <returns></returns>
        [Authorize]
        public ActionResultVM WriteEditSave(Domain.UserWriting mo, int UwId, string TagIds)
        {
            var vm = new ActionResultVM();

            var lisTagId = new List<int>();
            TagIds.Split(',').ToList().ForEach(x => lisTagId.Add(Convert.ToInt32(x)));

            var lisTagName = Func.Common.TagsQuery().Where(x => lisTagId.Contains(x.TagId)).ToList();

            int uid = new Func.UserAuthAid(HttpContext).Get().UserId;
            using (var db = new ContextBase())
            {
                var oldmo = db.UserWriting.Where(x => x.Uid == uid && x.UwId == UwId).FirstOrDefault();

                if (oldmo.UwStatus == -1)
                {
                    vm.Set(ARTag.unauthorized);
                }
                else if (oldmo != null)
                {
                    oldmo.UwTitle = mo.UwTitle;
                    oldmo.UwCategory = mo.UwCategory;
                    oldmo.UwContentMd = mo.UwContentMd;
                    oldmo.UwContent = mo.UwContent;
                    oldmo.UwUpdateTime = DateTime.Now;

                    db.UserWriting.Update(oldmo);

                    var wt = db.UserWritingTags.Where(x => x.UwId == UwId).ToList();
                    db.UserWritingTags.RemoveRange(wt);

                    var listwt = new List<Domain.UserWritingTags>();
                    foreach (var tag in lisTagId)
                    {
                        var wtmo = new Domain.UserWritingTags
                        {
                            UwId = mo.UwId,
                            TagId = tag,
                            TagName = lisTagName.Where(x => x.TagId == tag).FirstOrDefault().TagName
                        };

                        listwt.Add(wtmo);
                    }
                    db.UserWritingTags.AddRange(listwt);

                    int num = db.SaveChanges();

                    vm.Set(num > 0);
                }
            }

            return vm;
        }

        /// <summary>
        /// 删除 一篇文章
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        public ActionResultVM WriteDel(int id)
        {
            var vm = new ActionResultVM();

            int uid = new Func.UserAuthAid(HttpContext).Get().UserId;
            using (var db = new ContextBase())
            {
                var mo1 = db.UserWriting.Where(x => x.Uid == uid && x.UwId == id).FirstOrDefault();
                if (mo1.UwStatus == -1)
                {
                    vm.Set(ARTag.unauthorized);
                }
                else
                {
                    db.UserWriting.Remove(mo1);
                    var mo2 = db.UserWritingTags.Where(x => x.UwId == id).ToList();
                    db.UserWritingTags.RemoveRange(mo2);
                    var mo3 = db.UserReply.Where(x => x.UrTargetId == id.ToString()).ToList();
                    db.UserReply.RemoveRange(mo3);

                    vm.Set(db.SaveChanges() > 0);
                }
            }

            return vm;
        }

        #endregion

        #region 验证

        /// <summary>
        /// 验证
        /// </summary>
        /// <returns></returns>
        public IActionResult Verify()
        {
            var vm = new ActionResultVM();

            var id = RouteData.Values["id"]?.ToString().ToUpper();

            if (!string.IsNullOrWhiteSpace(id))
            {
                var uinfo = new Func.UserAuthAid(HttpContext).Get();

                switch (id.ToLower())
                {
                    //发送验证邮箱
                    case "send":
                        {
                            if (User.Identity.IsAuthenticated)
                            {
                                using var db = new ContextBase();
                                var usermo = db.UserInfo.Find(uinfo.UserId);
                                if (usermo.UserMailValid == 1)
                                {
                                    vm.msg = "邮箱已经完成验证";
                                }
                                else if (string.IsNullOrWhiteSpace(usermo.UserMail))
                                {
                                    vm.msg = "邮箱不能为空";
                                }
                                else
                                {
                                    var cacheKey = "Global_VerifyMail_" + usermo.UserMail;
                                    var issend = Core.CacheTo.Get(cacheKey) as bool?;
                                    if (issend == true)
                                    {
                                        vm.msg = "1分钟内只能发送一次验证信息";
                                    }
                                    else
                                    {
                                        var tml = Core.FileTo.ReadText(GlobalTo.WebRootPath + "/lib/mailchecker/", "list.txt");
                                        if (tml.Contains(usermo.UserMail.Split('@').LastOrDefault()))
                                        {
                                            vm.msg = "该邮箱已被屏蔽";
                                        }
                                        else
                                        {
                                            //发送验证

                                            var ToMail = usermo.UserMail;

                                            var vjson = new
                                            {
                                                mail = ToMail,
                                                ts = DateTime.Now.ToTimestamp()
                                            }.ToJson();
                                            var vcode = Core.CalcTo.EnDES(vjson, GlobalTo.GetValue("VerifyCode:Key")).ToLower();

                                            var VerifyLink = string.Format(GlobalTo.GetValue("VerifyCode:Url"), vcode);

                                            var txt = Core.FileTo.ReadText(GlobalTo.WebRootPath + "/template/", "sendmailverify.html");
                                            txt = txt.Replace("@ToMail@", ToMail).Replace("@VerifyLink@", VerifyLink);

                                            vm = Func.MailAid.Send(ToMail, "[Netnr] 验证你的邮箱", txt);

                                            if (vm.code == 200)
                                            {
                                                vm.msg = "已发送成功";
                                                Core.CacheTo.Set(cacheKey, true, 60, false);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                vm.msg = "请登录";
                            }
                        }
                        break;

                    //验证邮箱
                    default:
                        try
                        {
                            var vjson = Core.CalcTo.DeDES(id, GlobalTo.GetValue("VerifyCode:Key")).ToJObject();
                            if (DateTime.Now.ToTimestamp() - Convert.ToInt32(vjson["ts"]) < 60 * 5)
                            {
                                var mail = vjson["mail"].ToString();
                                if (string.IsNullOrWhiteSpace(mail))
                                {
                                    vm.msg = "邮件地址有误";
                                }
                                else
                                {
                                    using var db = new ContextBase();
                                    var usermo = db.UserInfo.FirstOrDefault(x => x.UserMail == mail);
                                    if (usermo != null)
                                    {
                                        if (usermo.UserMailValid == 1)
                                        {
                                            vm.msg = "已验证，勿重复验证";
                                        }
                                        else
                                        {
                                            usermo.UserMailValid = 1;

                                            db.UserInfo.Update(usermo);

                                            int num = db.SaveChanges();

                                            vm.Set(num > 0);
                                            if (vm.code == 200)
                                            {
                                                vm.msg = "恭喜你，验证成功";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        vm.msg = "邮件地址无效";
                                    }
                                }
                            }
                            else
                            {
                                vm.msg = "链接已过期（5分钟内有效）";
                            }
                        }
                        catch (Exception)
                        {
                            vm.msg = "链接已失效";
                        }
                        break;
                }
            }
            else
            {
                vm.msg = "缺失验证码信息";
            }

            return View(vm);
        }

        #endregion
    }
}