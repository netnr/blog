using Netnr.Blog.Data;
using Netnr.SharedFast;

namespace Netnr.Blog.Web.Areas.Draw.Controllers
{
    [Area("Draw")]
    public class CodeController : Controller
    {
        public ContextBase db;

        public CodeController(ContextBase cb)
        {
            db = cb;
        }

        /// <summary>
        /// 首页
        /// </summary>
        /// <param name="code">分享码</param>
        /// <param name="filename"></param>
        /// <param name="xml"></param>
        /// <param name="mof"></param>
        /// <returns></returns>
        public IActionResult Index(string code, string filename, string xml, Domain.Draw mof)
        {
            var id = RouteData.Values["id"]?.ToString();
            var sid = RouteData.Values["sid"]?.ToString();

            var kid = string.Empty;
            if (id?.Length == 20)
            {
                kid = id;
            }
            else if (sid?.Length == 20)
            {
                kid = sid;
            }
            if (!string.IsNullOrEmpty(kid))
            {
                var sck = "SharedCode_" + kid;
                //有分享码
                if (!string.IsNullOrWhiteSpace(code))
                {
                    Response.Cookies.Append(sck, code);
                }
                else
                {
                    code = Request.Cookies[sck]?.ToString();
                }
            }

            var uinfo = Apps.LoginService.Get(HttpContext);

            if (!string.IsNullOrWhiteSpace(filename))
            {
                filename = filename.ToUrlDecode();
            }
            if (!string.IsNullOrWhiteSpace(xml))
            {
                xml = xml.ToUrlDecode();
            }

            //新增、编辑
            if (id == "open")
            {
                //编辑
                if (!string.IsNullOrWhiteSpace(sid))
                {
                    var vm = new SharedResultVM();
                    var mo = db.Draw.Find(sid);

                    //分享码
                    var isShare = !string.IsNullOrWhiteSpace(mo?.Spare1) && mo?.Spare1 == code;
                    if (mo?.DrOpen == 1 || mo?.Uid == uinfo.UserId || isShare)
                    {
                        vm.Set(SharedEnum.RTag.success);
                        vm.Data = mo;
                    }
                    else
                    {
                        vm.Set(SharedEnum.RTag.unauthorized);
                    }
                    return Content(vm.ToJson());
                }
                return Ok();
            }
            //新增、编辑表单
            else if (id == "form")
            {
                object model = null;
                if (HttpContext.User.Identity.IsAuthenticated)
                {
                    if (!string.IsNullOrWhiteSpace(sid))
                    {
                        var mo = db.Draw.Find(sid);
                        if (mo.Uid == uinfo.UserId)
                        {
                            model = mo;
                        }
                    }
                }

                return View("_PartialDrawForm", model);
            }
            //保存标题等信息
            else if (id == "saveform")
            {
                var vm = Apps.LoginService.CompleteInfoValid(HttpContext);
                if (vm.Code == 200)
                {
                    int num = 0;
                    if (string.IsNullOrWhiteSpace(mof.DrId))
                    {
                        mof.DrId = mof.DrType[0] + Core.UniqueTo.LongId().ToString();
                        mof.DrCreateTime = DateTime.Now;
                        mof.Uid = uinfo.UserId;
                        mof.DrOrder = 100;
                        mof.DrStatus = 1;

                        db.Draw.Add(mof);
                        num = db.SaveChanges();
                    }
                    else
                    {
                        var newmo = db.Draw.Find(mof.DrId);
                        if (newmo.Uid != uinfo.UserId)
                        {
                            vm.Set(SharedEnum.RTag.unauthorized);
                        }
                        else
                        {
                            newmo.DrRemark = mof.DrRemark;
                            newmo.DrName = mof.DrName;
                            newmo.DrOpen = mof.DrOpen;
                            newmo.Spare1 = mof.Spare1;

                            db.Draw.Update(newmo);
                            num = db.SaveChanges();
                        }
                    }
                    vm.Set(num > 0);
                }

                if (vm.Code == 200)
                {
                    return Redirect("/draw/user/" + uinfo?.UserId);
                }
                else
                {
                    return Content(vm.Msg);
                }
            }
            //保存内容
            else if (id == "save")
            {
                var vm = Apps.LoginService.CompleteInfoValid(HttpContext);
                if (vm.Code == 200)
                {
                    //新增
                    if (string.IsNullOrWhiteSpace(sid))
                    {
                        var mo = new Domain.Draw
                        {
                            DrName = filename,
                            DrContent = xml,

                            DrId = mof.DrType[0] + Core.UniqueTo.LongId().ToString(),
                            DrType = mof.DrType,
                            DrCreateTime = DateTime.Now,
                            DrOpen = 1,
                            DrOrder = 100,
                            DrStatus = 1,
                            Uid = uinfo.UserId
                        };

                        db.Draw.Add(mo);

                        var num = db.SaveChanges();
                        vm.Set(num > 0);
                        vm.Data = mo.DrId;
                    }
                    else
                    {
                        var mo = db.Draw.Find(sid);
                        if (mo?.Uid == uinfo.UserId)
                        {
                            mo.DrName = filename;
                            mo.DrContent = xml;

                            db.Draw.Update(mo);

                            var num = db.SaveChanges();
                            vm.Set(num > 0);
                        }
                        else
                        {
                            vm.Set(SharedEnum.RTag.unauthorized);
                        }
                    }
                }

                return Content(vm.ToJson());
            }
            //删除
            else if (id == "del")
            {
                var vm = new SharedResultVM();

                if (User.Identity.IsAuthenticated)
                {
                    var mo = db.Draw.Find(sid);
                    if (mo.Uid == uinfo.UserId)
                    {
                        db.Remove(mo);
                        int num = db.SaveChanges();

                        vm.Set(num > 0);
                    }
                    else
                    {
                        vm.Set(SharedEnum.RTag.unauthorized);
                    }
                }
                else
                {
                    vm.Set(SharedEnum.RTag.unauthorized);
                }

                if (vm.Code == 200)
                {
                    return Redirect("/draw/discover");
                }
                else
                {
                    return Content(vm.ToJson());
                }
            }
            //插入图片
            else if (id == "upload")
            {
                var errno = -1;
                var msg = "fail";
                var url = "";

                var subdir = GlobalTo.GetValue("StaticResource:DrawPath");
                var vm = new Web.Controllers.api.APIController().Upload(Request.Form.Files[0], subdir);

                if (vm.Code == 200)
                {
                    var jd = vm.Data.ToJson().ToJObject();
                    url = jd["server"].ToString() + jd["path"].ToString();
                    errno = 0;
                    msg = "ok";
                }

                return Content(new
                {
                    errno,
                    msg,
                    data = new
                    {
                        url
                    }
                }.ToJson());
            }

            ViewData["vid"] = id;

            var vname = string.Format("_Partial{0}View", id.StartsWith('m') ? "Mind" : "Draw");
            return View(vname);
        }
    }
}