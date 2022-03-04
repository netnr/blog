using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Netnr.Blog.Data;
using Netnr.Core;
using Netnr.Login;
using Netnr.SharedFast;

namespace Netnr.Blog.Web.Controllers
{
    /// <summary>
    /// 个人用户
    /// </summary>
    public class UserController : Controller
    {
        public ContextBase db;

        public UserController(ContextBase cb)
        {
            db = cb;
        }

        #region 消息

        /// <summary>
        /// 消息
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [Authorize]
        public IActionResult Message(int page = 1)
        {
            var uinfo = Apps.LoginService.Get(HttpContext);

            var vm = Application.CommonService.MessageQuery(uinfo.UserId, Application.EnumService.MessageType.UserWriting, null, page);
            vm.Route = Request.Path;

            if (page == 1)
            {
                var listum = db.UserMessage.Where(x => x.UmType == Application.EnumService.MessageType.UserWriting.ToString() && x.UmAction == 2 && x.UmStatus == 1).ToList();
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
            var vm = new SharedResultVM();

            var id = RouteData.Values["id"]?.ToString();
            if (!string.IsNullOrWhiteSpace(id))
            {
                var uinfo = Apps.LoginService.Get(HttpContext);

                var um = db.UserMessage.Find(id);
                if (um == null)
                {
                    vm.Set(SharedEnum.RTag.lack);
                }
                else if (um?.Uid != uinfo.UserId)
                {
                    vm.Set(SharedEnum.RTag.unauthorized);
                }
                else
                {
                    db.UserMessage.Remove(um);
                    int num = db.SaveChanges();

                    vm.Set(num > 0);
                }
            }

            if (vm.Code == 200)
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
        public SharedResultVM UpdateUserSay(Domain.UserInfo mo)
        {
            var vm = new SharedResultVM();

            var uinfo = Apps.LoginService.Get(HttpContext);

            var currmo = db.UserInfo.Find(uinfo.UserId);
            currmo.UserSay = mo.UserSay;
            db.UserInfo.Update(currmo);

            int num = db.SaveChanges();

            vm.Set(num > 0);

            return vm;
        }

        /// <summary>
        /// 更新头像
        /// </summary>
        /// <param name="type"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        [Authorize]
        public SharedResultVM UpdateUserPhoto(string type, string source)
        {
            var vm = new SharedResultVM();

            try
            {
                vm = Apps.LoginService.CompleteInfoValid(HttpContext);
                if (vm.Code == 200)
                {
                    var uinfo = Apps.LoginService.Get(HttpContext);

                    //物理根路径
                    var prp = GlobalTo.GetValue("StaticResource:PhysicalRootPath").Replace("~", GlobalTo.ContentRootPath);
                    var ppath = PathTo.Combine(prp, GlobalTo.GetValue("StaticResource:AvatarPath"));

                    if (!Directory.Exists(ppath))
                    {
                        Directory.CreateDirectory(ppath);
                    }

                    if (string.IsNullOrWhiteSpace(uinfo.UserPhoto))
                    {
                        uinfo.UserPhoto = UniqueTo.LongId() + ".jpg";
                    }
                    var upname = uinfo.UserPhoto.Split('?')[0];
                    var npnew = upname + "?" + DateTime.Now.ToTimestamp();

                    switch (type)
                    {
                        case "file":
                            {
                                source = source[(source.LastIndexOf(",") + 1)..];
                                byte[] bytes = Convert.FromBase64String(source);
                                System.IO.File.WriteAllBytes(PathTo.Combine(ppath, upname), bytes);

                                var usermo = db.UserInfo.Find(uinfo.UserId);
                                usermo.UserPhoto = npnew;
                                db.UserInfo.Update(usermo);
                                int num = db.SaveChanges();
                                if (num > 0)
                                {
                                    using var ac = new AccountController(db);
                                    ac.SetAuth(HttpContext, usermo);
                                }

                                vm.Set(SharedEnum.RTag.success);
                            }
                            break;
                        case "link":
                            {
                                HttpTo.DownloadSave(HttpTo.HWRequest(source), PathTo.Combine(ppath, upname));

                                var usermo = db.UserInfo.Find(uinfo.UserId);
                                usermo.UserPhoto = npnew;
                                db.UserInfo.Update(usermo);
                                int num = db.SaveChanges();
                                if (num > 0)
                                {
                                    using var ac = new AccountController(db);
                                    ac.SetAuth(HttpContext, usermo);
                                }

                                vm.Set(SharedEnum.RTag.success);
                            }
                            break;
                    }
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
            var uinfo = Apps.LoginService.Get(HttpContext);

            var mo = db.UserInfo.Find(uinfo.UserId);

            return View(mo);
        }

        /// <summary>
        /// 保存个人信息
        /// </summary>
        /// <param name="mo"></param>
        /// <returns></returns>
        [Authorize]
        public SharedResultVM SaveUserInfo(Domain.UserInfo mo)
        {
            var vm = new SharedResultVM();

            if (string.IsNullOrWhiteSpace(mo.Nickname))
            {
                vm.Set(SharedEnum.RTag.refuse);
                vm.Msg = "昵称不能为空";

                return vm;
            }

            var uinfo = Apps.LoginService.Get(HttpContext);

            var usermo = db.UserInfo.Find(uinfo.UserId);

            //变更账号
            if (!string.IsNullOrWhiteSpace(mo.UserName) && usermo.UserNameChange != 1 && usermo.UserName != mo.UserName)
            {
                //账号重复
                if (db.UserInfo.Any(x => x.UserName == mo.UserName))
                {
                    vm.Set(SharedEnum.RTag.exist);
                    vm.Msg = "账号已经存在";

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
                    if (!ParsingTo.IsMail(mo.UserMail))
                    {
                        vm.Set(SharedEnum.RTag.invalid);
                        vm.Msg = "邮箱格式有误";

                        return vm;
                    }
                    else
                    {
                        if (db.UserInfo.Any(x => x.UserMail == mo.UserMail))
                        {
                            vm.Set(SharedEnum.RTag.exist);
                            vm.Msg = "邮箱已经存在";

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
            using (var ac = new AccountController(db))
            {
                ac.SetAuth(HttpContext, usermo, true);
            }

            vm.Set(num > 0);

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
            var url = Application.ThirdLoginService.LoginLink(authType, "bind");
            return Redirect(url);
        }

        /// <summary>
        /// 解绑账号
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public IActionResult RidOAuth()
        {
            if (Enum.TryParse(RouteData.Values["id"]?.ToString().ToLower(), true, out LoginBase.LoginType vtype))
            {
                var uinfo = Apps.LoginService.Get(HttpContext);
                var mo = db.UserInfo.Find(uinfo.UserId);

                switch (vtype)
                {
                    case LoginBase.LoginType.QQ:
                        mo.OpenId1 = "";
                        break;
                    case LoginBase.LoginType.WeiBo:
                        mo.OpenId2 = "";
                        break;
                    case LoginBase.LoginType.GitHub:
                        mo.OpenId3 = "";
                        break;
                    case LoginBase.LoginType.TaoBao:
                        mo.OpenId4 = "";
                        break;
                    case LoginBase.LoginType.MicroSoft:
                        mo.OpenId5 = "";
                        break;
                    case LoginBase.LoginType.DingTalk:
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
        public SharedResultVM UpdatePassword(string oldpwd, string newpwd)
        {
            var vm = new SharedResultVM();

            var uinfo = Apps.LoginService.Get(HttpContext);

            var userinfo = db.UserInfo.Find(uinfo.UserId);
            if (userinfo.UserPwd == CalcTo.MD5(oldpwd))
            {
                userinfo.UserPwd = CalcTo.MD5(newpwd);
                db.UserInfo.Update(userinfo);
                var num = db.SaveChanges();

                vm.Set(num > 0);
            }
            else
            {
                vm.Set(SharedEnum.RTag.unauthorized);
            }

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

                var uinfo = Apps.LoginService.Get(HttpContext);
                var vm = Application.CommonService.UserConnWritingQuery(uinfo.UserId, Application.EnumService.ConnectionType.UserWriting, action, page ?? 1);
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

            var pag = new SharedPaginationVM
            {
                PageNumber = page,
                PageSize = rows
            };

            var uinfo = Apps.LoginService.Get(HttpContext);

            var query = from a in db.UserWriting
                        where a.Uid == uinfo.UserId
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
                query = GlobalTo.TDB switch
                {
                    SharedEnum.TypeDB.SQLite => query.Where(x => EF.Functions.Like(x.UwTitle, $"%{pe1}%")),
                    SharedEnum.TypeDB.PostgreSQL => query.Where(x => EF.Functions.ILike(x.UwTitle, $"%{pe1}%")),
                    _ => query.Where(x => x.UwTitle.Contains(pe1)),
                };
            }

            query = QueryableTo.OrderBy(query, sort, order);

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
        public SharedResultVM WriteOne(int id)
        {
            var vm = new SharedResultVM();

            var uinfo = Apps.LoginService.Get(HttpContext);

            var mo = db.UserWriting.FirstOrDefault(x => x.Uid == uinfo.UserId && x.UwId == id);
            var listTags = db.UserWritingTags.Where(x => x.UwId == id).ToList();

            vm.Data = new
            {
                item = mo,
                tags = listTags
            };
            vm.Set(SharedEnum.RTag.success);

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
        public SharedResultVM WriteEditSave(Domain.UserWriting mo, int UwId, string TagIds)
        {
            var vm = new SharedResultVM();

            try
            {
                var lisTagId = new List<int>();
                TagIds.Split(',').ToList().ForEach(x => lisTagId.Add(Convert.ToInt32(x)));

                var lisTagName = Application.CommonService.TagsQuery().Where(x => lisTagId.Contains(x.TagId)).ToList();

                var uinfo = Apps.LoginService.Get(HttpContext);

                var oldmo = db.UserWriting.FirstOrDefault(x => x.Uid == uinfo.UserId && x.UwId == UwId);

                if (oldmo.UwStatus == -1)
                {
                    vm.Set(SharedEnum.RTag.unauthorized);
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
            catch (Exception ex)
            {
                ConsoleTo.Log(ex);
                vm.Set(ex);
            }

            return vm;
        }

        /// <summary>
        /// 删除 一篇文章
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        public SharedResultVM WriteDel(int id)
        {
            var vm = new SharedResultVM();

            var uinfo = Apps.LoginService.Get(HttpContext);

            var mo1 = db.UserWriting.FirstOrDefault(x => x.Uid == uinfo.UserId && x.UwId == id);
            if (mo1.UwStatus == -1)
            {
                vm.Set(SharedEnum.RTag.unauthorized);
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
            var vm = new SharedResultVM();

            var id = RouteData.Values["id"]?.ToString();

            if (!string.IsNullOrWhiteSpace(id))
            {
                var uinfo = Apps.LoginService.Get(HttpContext);

                var vcurl = GlobalTo.GetValue("VerifyCode:Url");
                var vckey = GlobalTo.GetValue("VerifyCode:Key");

                switch (id.ToLower())
                {
                    //发送验证邮箱
                    case "send":
                        {
                            if (User.Identity.IsAuthenticated)
                            {
                                var usermo = db.UserInfo.Find(uinfo.UserId);
                                if (usermo.UserMailValid == 1)
                                {
                                    vm.Msg = "邮箱已经完成验证";
                                }
                                else if (string.IsNullOrWhiteSpace(usermo.UserMail))
                                {
                                    vm.Msg = "邮箱不能为空";
                                }
                                else
                                {
                                    var cacheKey = "Global_VerifyMail_" + usermo.UserId;
                                    var issend = CacheTo.Get(cacheKey) as bool?;
                                    if (issend == true)
                                    {
                                        vm.Msg = "5分钟内只能发送一次验证信息";
                                    }
                                    else
                                    {
                                        var mcs = FileTo.ReadText(GlobalTo.WebRootPath + "/lib/mailchecker/list.txt").Split(Environment.NewLine);
                                        if (mcs.Contains(usermo.UserMail.Split('@').LastOrDefault().ToLower()))
                                        {
                                            vm.Msg = "该邮箱已被屏蔽";
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

                                            var vcode = CalcTo.AESEncrypt(vjson, vckey).ToBase64Encode().ToUrlEncode();
                                            var VerifyLink = string.Format(vcurl, vcode);

                                            var txt = FileTo.ReadText(GlobalTo.WebRootPath + "/template/sendmailverify.html");
                                            txt = txt.Replace("@ToMail@", ToMail).Replace("@VerifyLink@", VerifyLink);

                                            vm = Application.MailService.Send(ToMail, $"[{GlobalTo.GetValue("Common:EnglishName")}] 验证你的邮箱", txt);

                                            if (vm.Code == 200)
                                            {
                                                vm.Msg = "已发送成功";
                                                CacheTo.Set(cacheKey, true, 300, false);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                vm.Msg = "请登录";
                            }
                        }
                        break;

                    //验证邮箱
                    default:
                        try
                        {
                            var vjson = CalcTo.AESDecrypt(id.ToUrlDecode().ToBase64Decode(), vckey).ToJObject();
                            if (DateTime.Now.ToTimestamp() - Convert.ToInt32(vjson["ts"]) < 60 * 5)
                            {
                                var mail = vjson["mail"].ToString();
                                if (string.IsNullOrWhiteSpace(mail))
                                {
                                    vm.Msg = "邮件地址有误";
                                }
                                else
                                {
                                    var usermo = db.UserInfo.FirstOrDefault(x => x.UserMail == mail);
                                    if (usermo != null)
                                    {
                                        if (usermo.UserMailValid == 1)
                                        {
                                            vm.Msg = "已验证，勿重复验证";
                                        }
                                        else
                                        {
                                            usermo.UserMailValid = 1;

                                            db.UserInfo.Update(usermo);

                                            int num = db.SaveChanges();

                                            vm.Set(num > 0);
                                            if (vm.Code == 200)
                                            {
                                                vm.Msg = "恭喜你，验证成功";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        vm.Msg = "邮件地址无效";
                                    }
                                }
                            }
                            else
                            {
                                vm.Msg = "链接已过期（5分钟内有效）";
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            vm.Msg = "链接已失效";
                        }
                        break;
                }
            }
            else
            {
                vm.Msg = "缺失验证码信息";
            }

            return View(vm);
        }

        #endregion
    }
}