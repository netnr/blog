using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Netnr.Domain;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Netnr.Web.Filters;
using Netnr.Data;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using Netnr.Func.ViewModel;
using System.ComponentModel;
using Microsoft.AspNetCore.Cors;

namespace Netnr.Web.Controllers
{
    /// <summary>
    /// 主体
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// 首页
        /// </summary>
        /// <param name="k">搜索</param>
        /// <param name="page">页码</param>
        /// <returns></returns>
        [ResponseCache(Duration = 5)]
        public IActionResult Index(string k, int page = 1)
        {
            var vm = Func.Common.UserWritingQuery(k, page);
            vm.Route = Request.Path;

            return View("_PartialViewWriting", vm);
        }

        /// <summary>
        /// 标签分类
        /// </summary>
        /// <param name="k">搜索</param>
        /// <param name="page">页码</param>
        /// <returns></returns>
        [ResponseCache(Duration = 5)]
        public IActionResult Type(string k, int page = 1)
        {
            string tag = RouteData.Values["id"]?.ToString();
            if (string.IsNullOrEmpty(tag))
            {
                return new RedirectResult("/");
            }
            else
            {
                var vm = Func.Common.UserWritingQuery(k, page, tag);
                vm.Route = Request.Path;

                return View("_PartialViewWriting", vm);
            }
        }

        /// <summary>
        /// 标签
        /// </summary>
        /// <returns></returns>
        public IActionResult Tags()
        {
            var tags = Func.Common.TagsQuery().Select(x => new
            {
                x.TagName,
                x.TagIcon
            }).ToJson();

            ViewData["tags"] = tags;

            return View();
        }

        /// <summary>
        /// 写
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [FilterConfigs.IsValidMail]
        public IActionResult Write()
        {
            return View();
        }

        /// <summary>
        /// 搜索标签
        /// </summary>
        /// <param name="keys">搜索内容</param>
        /// <returns></returns>
        public string TagSelectSearch(string keys)
        {
            var list = new List<Tags>();
            if (!string.IsNullOrWhiteSpace(keys))
            {
                keys = keys.ToLower();
                list = Func.Common.TagsQuery().Where(x => x.TagName.Contains(keys)).Take(7).ToList();
            }
            return list.ToJson();
        }

        /// <summary>
        /// 保存文章
        /// </summary>
        /// <param name="mo">文章信息</param>
        /// <param name="TagIds">标签，多个逗号分割</param>
        /// <returns></returns>
        [Authorize]
        public ActionResultVM WriteSave(UserWriting mo, string TagIds)
        {
            var vm = new ActionResultVM();

            var uinfo = new Func.UserAuthAid(HttpContext).Get();

            using (var db = new ContextBase())
            {
                //已验证邮箱
                uinfo = db.UserInfo.Find(uinfo.UserId);
                if (uinfo.UserId != 1 && uinfo.UserMailValid != 1)
                {
                    vm.Set(ARTag.unauthorized);
                    vm.msg = "请验证邮箱后再操作";

                    return vm;
                }

                //有昵称
                if (string.IsNullOrWhiteSpace(uinfo.Nickname))
                {
                    vm.Set(ARTag.refuse);
                    vm.msg = "请填写昵称后再操作";

                    return vm;
                }

                var lisTagId = new List<int>();
                TagIds.Split(',').ToList().ForEach(x => lisTagId.Add(Convert.ToInt32(x)));

                var lisTagName = Func.Common.TagsQuery().Where(x => lisTagId.Contains(x.TagId)).ToList();

                mo.Uid = uinfo.UserId;
                mo.UwCreateTime = DateTime.Now;
                mo.UwUpdateTime = mo.UwCreateTime;
                mo.UwLastUid = mo.Uid;
                mo.UwLastDate = mo.UwCreateTime;
                mo.UwReplyNum = 0;
                mo.UwReadNum = 0;
                mo.UwOpen = 1;
                mo.UwLaud = 0;
                mo.UwMark = 0;
                mo.UwStatus = 1;

                db.UserWriting.Add(mo);
                db.SaveChanges();

                var listwt = new List<UserWritingTags>();
                foreach (var tag in lisTagId)
                {
                    var wtmo = new UserWritingTags
                    {
                        UwId = mo.UwId,
                        TagId = tag,
                        TagName = lisTagName.Where(x => x.TagId == tag).FirstOrDefault().TagName
                    };

                    listwt.Add(wtmo);
                }
                db.UserWritingTags.AddRange(listwt);

                //标签热点+1
                var listTagId = listwt.Select(x => x.TagId.Value);
                var listTags = db.Tags.Where(x => listTagId.Contains(x.TagId)).ToList();
                listTags.ForEach(x => x.TagHot += 1);
                db.Tags.UpdateRange(listTags);

                int num = db.SaveChanges();

                vm.data = mo.UwId;
                vm.Set(num > 0);
            }

            return vm;
        }

        /// <summary>
        /// 一篇
        /// </summary>
        /// <param name="page">页码</param>
        /// <returns></returns>
        [ResponseCache(Duration = 5)]
        public IActionResult List(int page = 1)
        {
            if (int.TryParse(RouteData.Values["Id"]?.ToString(), out int wid))
            {
                var uwo = Func.Common.UserWritingOneQuery(wid);
                if (uwo == null)
                {
                    return Redirect("/");
                }

                var pag = new PaginationVM
                {
                    PageNumber = Math.Max(page, 1),
                    PageSize = 10
                };

                var vm = new PageVM()
                {
                    Rows = Func.Common.ReplyOneQuery(Func.EnumAid.ReplyType.UserWriting, wid.ToString(), pag),
                    Pag = pag,
                    Temp = uwo,
                    Route = "/home/list/" + wid.ToString()
                };


                if (User.Identity.IsAuthenticated)
                {
                    var uinfo = new Func.UserAuthAid(HttpContext).Get();
                    using var db = new ContextBase();
                    var listuc = db.UserConnection.Where(x => x.Uid == uinfo.UserId && x.UconnTargetType == Func.EnumAid.ConnectionType.UserWriting.ToString() && x.UconnTargetId == wid.ToString()).ToList();

                    ViewData["uca1"] = listuc.Any(x => x.UconnAction == 1) ? "yes" : "";
                    ViewData["uca2"] = listuc.Any(x => x.UconnAction == 2) ? "yes" : "";
                }

                return View(vm);
            }
            else
            {
                return Redirect("/");
            }
        }

        /// <summary>
        /// 回复
        /// </summary>
        /// <param name="mo">回复信息</param>
        /// <param name="um">消息通知</param>
        /// <returns></returns>
        public ActionResultVM LsitReplySave(UserReply mo, UserMessage um)
        {
            var vm = new ActionResultVM();

            var uinfo = new Func.UserAuthAid(HttpContext).Get();

            if ((uinfo.UserId != 0 && string.IsNullOrWhiteSpace(uinfo.Nickname)) || (uinfo.UserId == 0 && string.IsNullOrWhiteSpace(mo.UrAnonymousName)))
            {
                vm.Set(ARTag.refuse);
                vm.msg = "昵称不能为空";

                return vm;
            }

            mo.Uid = uinfo.UserId;

            var now = DateTime.Now;

            //回复消息
            um.UmId = Core.UniqueTo.LongId().ToString();
            um.UmTriggerUid = mo.Uid;
            um.UmType = Func.EnumAid.MessageType.UserWriting.ToString();
            um.UmTargetId = mo.UrTargetId;
            um.UmAction = 2;
            um.UmStatus = 1;
            um.UmContent = mo.UrContent;
            um.UmCreateTime = now;

            using (var db = new ContextBase())
            {
                //回复内容
                mo.UrCreateTime = now;
                mo.UrStatus = 1;
                mo.UrTargetPid = 0;
                mo.UrTargetType = Func.EnumAid.ReplyType.UserWriting.ToString();

                mo.UrAnonymousLink = Fast.ParsingTo.JsSafeJoin(mo.UrAnonymousLink);

                db.UserReply.Add(mo);

                //回填文章最新回复记录
                var mow = db.UserWriting.FirstOrDefault(x => x.UwId.ToString() == mo.UrTargetId);
                if (mow != null)
                {
                    mow.UwReplyNum += 1;
                    mow.UwLastUid = mo.Uid;
                    mow.UwLastDate = now;

                    um.UmTargetIndex = mow.UwReplyNum;

                    db.UserWriting.Update(mow);
                }

                if (um.Uid != um.UmTriggerUid)
                {
                    db.UserMessage.Add(um);
                }

                int num = db.SaveChanges();

                vm.Set(num > 0);
            }

            return vm;
        }

        /// <summary>
        /// 点赞收藏
        /// </summary>
        /// <param name="a">动作</param>
        /// <returns></returns>
        [Authorize]
        public ActionResultVM ListUserConn(int a)
        {
            var vm = new ActionResultVM();

            int wid = Convert.ToInt32(RouteData.Values["id"]?.ToString());

            var uinfo = new Func.UserAuthAid(HttpContext).Get();

            using (var db = new ContextBase())
            {
                var uw = db.UserWriting.Find(wid);

                var uc = db.UserConnection.Where(x => x.Uid == uinfo.UserId && x.UconnTargetId == wid.ToString() && x.UconnAction == a).FirstOrDefault();
                if (uc == null)
                {
                    uc = new UserConnection()
                    {
                        UconnId = Core.UniqueTo.LongId().ToString(),
                        UconnAction = a,
                        UconnCreateTime = DateTime.Now,
                        UconnTargetId = wid.ToString(),
                        UconnTargetType = Func.EnumAid.ConnectionType.UserWriting.ToString(),
                        Uid = uinfo.UserId
                    };
                    db.UserConnection.Add(uc);
                    if (a == 1)
                    {
                        uw.UwLaud += 1;
                    }
                    if (a == 2)
                    {
                        uw.UwMark += 1;
                    }
                    db.UserWriting.Update(uw);

                    vm.data = "1";
                }
                else
                {
                    db.UserConnection.Remove(uc);
                    if (a == 1)
                    {
                        uw.UwLaud -= 1;
                    }
                    if (a == 2)
                    {
                        uw.UwMark -= 1;
                    }
                    db.UserWriting.Update(uw);

                    vm.data = "0";
                }

                int num = db.SaveChanges();

                vm.Set(num > 0);
            }

            return vm;
        }

        /// <summary>
        /// 阅读追加
        /// </summary>
        public void ListReadPlus()
        {
            int wid = Convert.ToInt32(RouteData.Values["id"]?.ToString());
            using var db = new ContextBase();
            var mo = db.UserWriting.Find(wid);
            if (mo != null)
            {
                mo.UwReadNum += 1;
                db.UserWriting.Update(mo);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 授权访问
        /// </summary>
        /// <param name="returnUrl">跳转链接</param>
        /// <param name="sk">密钥</param>
        /// <returns></returns>
        public IActionResult Auth(string returnUrl, string sk)
        {
            if (!string.IsNullOrWhiteSpace(sk))
            {
                bool b = FilterConfigs.HelpFuncTo.LocalIsAuth(sk);
                if (b)
                {
                    Response.Cookies.Append("sk", sk);

                    returnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;

                    return new RedirectResult(returnUrl);
                }
                else
                {
                    ViewData["AuthResult"] = "SK无效";
                }
            }
            return View();
        }

        /// <summary>
        /// 请先验证
        /// </summary>
        /// <returns></returns>
        public IActionResult Valid()
        {
            return View();
        }

        /// <summary>
        /// 全局错误页面
        /// </summary>
        /// <param name="msg">错误消息</param>
        /// <returns></returns>
        public IActionResult Error(string msg)
        {
            TempData["msg"] = msg;
            return View();
        }
    }
}
