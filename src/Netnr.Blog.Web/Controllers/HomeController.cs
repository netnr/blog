using Netnr.Blog.Domain;
using Microsoft.AspNetCore.Authorization;
using Netnr.Core;
using Netnr.Blog.Data;

namespace Netnr.Blog.Web.Controllers
{
    /// <summary>
    /// 主体
    /// </summary>
    public class HomeController : Controller
    {
        public ContextBase db;

        public HomeController(ContextBase cb)
        {
            db = cb;
        }

        /// <summary>
        /// 首页
        /// </summary>
        /// <param name="k">搜索</param>
        /// <param name="page">页码</param>
        /// <returns></returns>
        public IActionResult Index(string k, int page = 1)
        {
            var ckey = $"Writing-{page}";
            if (!string.IsNullOrWhiteSpace(k) || CacheTo.Get(ckey) is not SharedPageVM vm)
            {
                vm = Application.CommonService.UserWritingQuery(k, page);
                vm.Route = Request.Path;

                if (string.IsNullOrWhiteSpace(k))
                {
                    CacheTo.Set(ckey, vm, 30, false);
                }
            }

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
                var vm = Application.CommonService.UserWritingQuery(k, page, tag);
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
            var tags = Application.CommonService.TagsQuery().Select(x => new
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
        public IActionResult Write()
        {
            return View();
        }

        /// <summary>
        /// 标签
        /// </summary>
        /// <returns></returns>
        public List<Tags> TagSelect()
        {
            var list = Application.CommonService.TagsQuery();
            return list;
        }

        /// <summary>
        /// 保存文章
        /// </summary>
        /// <param name="mo">文章信息</param>
        /// <param name="TagIds">标签，多个逗号分割</param>
        /// <returns></returns>
        [Authorize]
        public SharedResultVM WriteSave(UserWriting mo, string TagIds)
        {
            var vm = new SharedResultVM();

            try
            {
                vm = Apps.LoginService.CompleteInfoValid(HttpContext);
                if (vm.Code == 200)
                {
                    var uinfo = Apps.LoginService.Get(HttpContext);

                    var lisTagId = new List<int>();
                    TagIds.Split(',').ToList().ForEach(x => lisTagId.Add(Convert.ToInt32(x)));

                    var lisTagName = Application.CommonService.TagsQuery().Where(x => lisTagId.Contains(x.TagId)).ToList();

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
                            TagName = lisTagName.FirstOrDefault(x => x.TagId == tag).TagName
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

                    vm.Data = mo.UwId;
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
        /// 一篇
        /// </summary>
        /// <param name="page">页码</param>
        /// <returns></returns>
        [ResponseCache(Duration = 5)]
        public IActionResult List(int page = 1)
        {
            if (int.TryParse(RouteData.Values["Id"]?.ToString(), out int wid))
            {
                var uwo = Application.CommonService.UserWritingOneQuery(wid);
                if (uwo == null)
                {
                    return Redirect("/");
                }

                var pag = new SharedPaginationVM
                {
                    PageNumber = Math.Max(page, 1),
                    PageSize = 10
                };

                var vm = new SharedPageVM()
                {
                    Rows = Application.CommonService.ReplyOneQuery(Application.EnumService.ReplyType.UserWriting, wid.ToString(), pag),
                    Pag = pag,
                    Temp = uwo,
                    Route = "/home/list/" + wid.ToString()
                };


                if (User.Identity.IsAuthenticated)
                {
                    var uinfo = Apps.LoginService.Get(HttpContext);
                    var listuc = db.UserConnection.Where(x => x.Uid == uinfo.UserId && x.UconnTargetType == Application.EnumService.ConnectionType.UserWriting.ToString() && x.UconnTargetId == wid.ToString()).ToList();

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
        /// 回复（关闭匿名回复）
        /// </summary>
        /// <param name="mo">回复信息</param>
        /// <param name="um">消息通知</param>
        /// <returns></returns>
        [Authorize]
        public SharedResultVM LsitReplySave(UserReply mo, UserMessage um)
        {
            var vm = new SharedResultVM();
            vm = Apps.LoginService.CompleteInfoValid(HttpContext);
            if (vm.Code == 200)
            {
                if (!mo.Uid.HasValue || string.IsNullOrWhiteSpace(mo.UrContent) || string.IsNullOrWhiteSpace(mo.UrTargetId))
                {
                    vm.Set(SharedEnum.RTag.lack);
                }
                else
                {
                    var uinfo = Apps.LoginService.Get(HttpContext);
                    mo.Uid = uinfo.UserId;

                    var now = DateTime.Now;

                    //回复消息
                    um.UmId = UniqueTo.LongId().ToString();
                    um.UmTriggerUid = mo.Uid;
                    um.UmType = Application.EnumService.MessageType.UserWriting.ToString();
                    um.UmTargetId = mo.UrTargetId;
                    um.UmAction = 2;
                    um.UmStatus = 1;
                    um.UmContent = mo.UrContent;
                    um.UmCreateTime = now;

                    //回复内容
                    mo.UrCreateTime = now;
                    mo.UrStatus = 1;
                    mo.UrTargetPid = 0;
                    mo.UrTargetType = Application.EnumService.ReplyType.UserWriting.ToString();

                    mo.UrAnonymousLink = ParsingTo.JsSafeJoin(mo.UrAnonymousLink);

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
            }

            return vm;
        }

        /// <summary>
        /// 点赞收藏
        /// </summary>
        /// <param name="a">动作</param>
        /// <returns></returns>
        [Authorize]
        public SharedResultVM ListUserConn(int a)
        {
            var vm = new SharedResultVM();

            int wid = Convert.ToInt32(RouteData.Values["id"]?.ToString());

            var uinfo = Apps.LoginService.Get(HttpContext);

            var uw = db.UserWriting.Find(wid);

            var uc = db.UserConnection.FirstOrDefault(x => x.Uid == uinfo.UserId && x.UconnTargetId == wid.ToString() && x.UconnAction == a);
            if (uc == null)
            {
                uc = new UserConnection()
                {
                    UconnId = UniqueTo.LongId().ToString(),
                    UconnAction = a,
                    UconnCreateTime = DateTime.Now,
                    UconnTargetId = wid.ToString(),
                    UconnTargetType = Application.EnumService.ConnectionType.UserWriting.ToString(),
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

                vm.Data = "1";
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

                vm.Data = "0";
            }

            int num = db.SaveChanges();

            vm.Set(num > 0);
            return vm;
        }

        /// <summary>
        /// 阅读追加
        /// </summary>
        public void ListReadPlus()
        {
            int wid = Convert.ToInt32(RouteData.Values["id"]?.ToString());
            var mo = db.UserWriting.Find(wid);
            if (mo != null)
            {
                mo.UwReadNum += 1;
                db.UserWriting.Update(mo);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 完善信息
        /// </summary>
        /// <returns></returns>
        public IActionResult CompleteInfo()
        {
            return View();
        }

        /// <summary>
        /// 全局错误页面
        /// </summary>
        /// <returns></returns>
        public IActionResult Error()
        {
            return View();
        }

        /// <summary>
        /// Swagger自定义样式
        /// </summary>
        /// <returns></returns>
        public IActionResult SwaggerCustomStyle()
        {
            var txt = @".opblock-options{display:none}.download-contents{width:auto !important}";

            return new ContentResult()
            {
                Content = txt,
                StatusCode = 200,
                ContentType = "text/css"
            };
        }
    }
}
