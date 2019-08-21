using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Netnr.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Netnr.Login;
using System.ComponentModel;
using Netnr.Func.ViewModel;

namespace Netnr.Web.Controllers
{
    public class AccountController : Controller
    {
        #region 注册

        [Description("注册")]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [Description("注册提交")]
        [HttpPost]
        public IActionResult Register(Domain.UserInfo mo, string RegisterCode)
        {
            var vm = new ActionResultVM();
            if (string.IsNullOrWhiteSpace(RegisterCode) || HttpContext.Session.GetString("RegisterCode") != RegisterCode)
            {
                vm.msg = "验证码错误或已过期";
            }
            else if (!(mo.UserName?.Length >= 5 && mo.UserPwd?.Length >= 5))
            {
                vm.msg = "账号、密码长度至少 5 位数";
            }
            else
            {
                mo.UserPwd = Core.CalcTo.MD5(mo.UserPwd);
                mo.UserCreateTime = DateTime.Now;

                //邮箱注册
                if (Fast.RegexTo.IsMail(mo.UserName))
                {
                    mo.UserMail = mo.UserName;
                }
                vm = RegisterUser(mo);
            }

            ViewData["UserName"] = mo.UserName;

            return View(vm);
        }

        [Description("注册验证码")]
        public FileResult RegisterCode()
        {
            //生成验证码
            string num = Core.RandomTo.NumCode(4);
            HttpContext.Session.SetString("RegisterCode", num);
            byte[] bytes = Fast.ImageTo.CreateImg(num);
            return File(bytes, "image/jpeg");
        }

        [Description("公共注册")]
        private ActionResultVM RegisterUser(Domain.UserInfo mo)
        {
            var vm = new ActionResultVM();

            using (var db = new ContextBase())
            {
                var isok = true;

                //邮箱注册
                if (!string.IsNullOrWhiteSpace(mo.UserMail))
                {
                    isok = !db.UserInfo.Any(x => x.UserName == mo.UserName || x.UserMail == mo.UserMail);

                    vm.Set(ARTag.exist);
                    vm.msg = "该邮箱已经注册";
                }
                else
                {
                    isok = !db.UserInfo.Any(x => x.UserName == mo.UserName);

                    vm.Set(ARTag.exist);
                    vm.msg = "该账号已经注册";
                }

                if (isok)
                {
                    db.UserInfo.Add(mo);
                    int num = db.SaveChanges();
                    vm.Set(num > 0);
                }
            }

            return vm;
        }

        #endregion

        #region 登录（本地）

        [Description("登录页面")]
        [HttpGet]
        public IActionResult Login(string ReturnUrl)
        {
            TempData["ReturnUrl"] = ReturnUrl;
            return View();
        }

        [Description("登录验证")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult Login(Domain.UserInfo mo, int? remember, string returnurl)
        {
            var isRemember = remember == 1;
            var vm = ValidateLogin(ValidateloginType.local, mo, isRemember);
            if (vm.code == 200)
            {
                var url = string.IsNullOrWhiteSpace(returnurl) ? "/" : returnurl;
                return Redirect(url);
            }
            else
            {
                return View(vm);
            }
        }

        [Description("第三方登录类型枚举")]
        public enum ValidateloginType
        {
            local, qq, weibo, github, taobao, microsoft
        }

        [Description("公共登录验证")]
        private ActionResultVM ValidateLogin(ValidateloginType vt, Domain.UserInfo mo, bool isremember = true)
        {
            var vm = new ActionResultVM();

            string sql = string.Empty;

            using (var db = new ContextBase())
            {
                var uiR = db.UserInfo;
                Domain.UserInfo outMo = new Domain.UserInfo();

                switch (vt)
                {
                    case ValidateloginType.local:
                        if (string.IsNullOrWhiteSpace(mo.UserName) || string.IsNullOrWhiteSpace(mo.UserPwd))
                        {
                            vm.msg = "用户名或密码不能为空";
                            return vm;
                        }
                        else
                        {
                            mo.UserPwd = Core.CalcTo.MD5(mo.UserPwd);

                            //邮箱登录
                            if (Fast.RegexTo.IsMail(mo.UserName))
                            {
                                outMo = uiR.FirstOrDefault(x => x.UserMail == mo.UserName && x.UserPwd == mo.UserPwd);
                            }
                            else
                            {
                                outMo = uiR.FirstOrDefault(x => x.UserName == mo.UserName && x.UserPwd == mo.UserPwd);
                            }
                        }
                        break;
                    case ValidateloginType.qq:
                        outMo = uiR.FirstOrDefault(x => x.OpenId1.Equals(mo.OpenId1));
                        break;
                    case ValidateloginType.weibo:
                        outMo = uiR.FirstOrDefault(x => x.OpenId2.Equals(mo.OpenId2));
                        break;
                    case ValidateloginType.github:
                        outMo = uiR.FirstOrDefault(x => x.OpenId3.Equals(mo.OpenId3));
                        break;
                    case ValidateloginType.taobao:
                        outMo = uiR.FirstOrDefault(x => x.OpenId4.Equals(mo.OpenId4));
                        break;
                    case ValidateloginType.microsoft:
                        outMo = uiR.FirstOrDefault(x => x.OpenId5.Equals(mo.OpenId5));
                        break;
                }

                if (outMo == null || outMo.UserId == 0)
                {
                    vm.msg = "用户名或密码错误";
                    return vm;
                }

                if (outMo.LoginLimit == 1)
                {
                    vm.msg = "用户已被禁止登录";
                    return vm;
                }

                //刷新登录标记
                outMo.UserLoginTime = DateTime.Now;
                outMo.UserSign = outMo.UserLoginTime.Value.ToTimestamp().ToString();
                uiR.Update(outMo);
                var num = db.SaveChanges();
                if (num < 1)
                {
                    vm.msg = "请求登录被拒绝";
                    return vm;
                }

                try
                {
                    //登录标记 缓存5分钟，绝对过期
                    Core.CacheTo.Set("UserSign", outMo.UserSign, 5 * 60, false);

                    //授权访问信息
                    new Func.UserAuthAid(HttpContext).Set(outMo, isremember);
                }
                catch (Exception ex)
                {
                    vm.Set(ex);
                    return vm;
                }

                vm.Set(ARTag.success);

                return vm;
            }
        }

        #endregion

        #region 登录（第三方）

        [Description("第三方登录授权页面")]
        public IActionResult Auth()
        {
            string url = string.Empty;

            if (Enum.TryParse(RouteData.Values["id"]?.ToString().ToLower(), out ValidateloginType vtype))
            {
                switch (vtype)
                {
                    case ValidateloginType.qq:
                        url = QQ.AuthorizationHref(new QQ_Authorization_RequestEntity());
                        break;
                    case ValidateloginType.weibo:
                        url = Weibo.AuthorizeHref(new Weibo_Authorize_RequestEntity());
                        break;
                    case ValidateloginType.github:
                        url = GitHub.AuthorizeHref(new GitHub_Authorize_RequestEntity());
                        break;
                    case ValidateloginType.taobao:
                        url = Taobao.AuthorizeHref(new Taobao_Authorize_RequestEntity());
                        break;
                    case ValidateloginType.microsoft:
                        url = MicroSoft.AuthorizeHref(new MicroSoft_Authorize_RequestEntity());
                        break;
                }
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                url = "/account/login";
            }

            //未登录
            if (!User.Identity.IsAuthenticated)
            {
                //删除绑定标识
                Response.Cookies.Delete("AccountBindOAuth");
            }
            return Redirect(url);
        }

        [Description("登录授权回调")]
        public IActionResult AuthCallback(string code)
        {
            var vm = new ActionResultVM();

            if (string.IsNullOrWhiteSpace(code))
            {
                vm.msg = "未授权，登录失败";
            }
            else
            {
                try
                {
                    //唯一标示
                    string openId = string.Empty;
                    //注册信息
                    var mo = new Domain.UserInfo()
                    {
                        LoginLimit = 0,
                        UserSex = 0,
                        UserCreateTime = DateTime.Now
                    };
                    //头像
                    string avatar = string.Empty;
                    //头像（高清）
                    string avatarhd = string.Empty;

                    if (Enum.TryParse(RouteData.Values["id"]?.ToString().ToLower(), out ValidateloginType vtype))
                    {
                        try
                        {
                            switch (vtype)
                            {
                                case ValidateloginType.qq:
                                    {
                                        //获取 access_token
                                        var tokenEntity = QQ.AccessToken(new QQ_AccessToken_RequestEntity()
                                        {
                                            code = code
                                        });

                                        //获取 OpendId
                                        var openidEntity = QQ.OpenId(new QQ_OpenId_RequestEntity()
                                        {
                                            access_token = tokenEntity.access_token
                                        });

                                        //获取 UserInfo
                                        var userEntity = QQ.OpenId_Get_User_Info(new QQ_OpenAPI_RequestEntity()
                                        {
                                            access_token = tokenEntity.access_token,
                                            openid = openidEntity.openid
                                        });

                                        //身份唯一标识
                                        openId = openidEntity.openid;
                                        mo.OpenId1 = openId;

                                        mo.Nickname = userEntity.nickname;
                                        mo.UserSex = userEntity.gender == "男" ? 1 : 2;
                                        mo.UserSay = "";
                                        mo.UserUrl = "";

                                        avatar = userEntity.figureurl_qq_1;
                                        avatarhd = userEntity.figureurl_qq_2;
                                    }
                                    break;
                                case ValidateloginType.weibo:
                                    {
                                        //获取 access_token
                                        var tokenEntity = Weibo.AccessToken(new Weibo_AccessToken_RequestEntity()
                                        {
                                            code = code
                                        });

                                        //获取 access_token 的授权信息
                                        var tokenInfoEntity = Weibo.GetTokenInfo(new Weibo_GetTokenInfo_RequestEntity()
                                        {
                                            access_token = tokenEntity.access_token
                                        });

                                        //获取 users/show
                                        var userEntity = Weibo.UserShow(new Weibo_UserShow_RequestEntity()
                                        {
                                            access_token = tokenEntity.access_token,
                                            uid = Convert.ToInt64(tokenInfoEntity.uid)
                                        });

                                        openId = tokenEntity.access_token;
                                        mo.OpenId2 = openId;

                                        mo.Nickname = userEntity.screen_name;
                                        mo.UserSex = userEntity.gender == "m" ? 1 : userEntity.gender == "f" ? 2 : 0;
                                        mo.UserSay = userEntity.description;
                                        mo.UserUrl = userEntity.domain;

                                        avatar = userEntity.profile_image_url;
                                        avatarhd = userEntity.avatar_large;
                                    }
                                    break;
                                case ValidateloginType.github:
                                    {
                                        //获取 access_token
                                        var tokenEntity = GitHub.AccessToken(new GitHub_AccessToken_RequestEntity()
                                        {
                                            code = code
                                        });

                                        //获取 user
                                        var userEntity = GitHub.User(new GitHub_User_RequestEntity()
                                        {
                                            access_token = tokenEntity.access_token
                                        });

                                        openId = userEntity.id.ToString();
                                        mo.OpenId3 = openId;

                                        mo.Nickname = userEntity.name;
                                        mo.UserSay = userEntity.bio;
                                        mo.UserUrl = userEntity.blog;
                                        mo.UserMail = userEntity.email;

                                        avatar = userEntity.avatar_url;
                                        avatarhd = userEntity.avatar_url;
                                    }
                                    break;
                                case ValidateloginType.taobao:
                                    {
                                        //获取 access_token
                                        var tokenEntity = Taobao.AccessToken(new Taobao_AccessToken_RequestEntity()
                                        {
                                            code = code
                                        });

                                        openId = tokenEntity.open_uid;
                                        mo.OpenId4 = openId;

                                        mo.Nickname = "淘宝用户";
                                    }
                                    break;
                                case ValidateloginType.microsoft:
                                    {
                                        //获取 access_token
                                        var tokenEntity = MicroSoft.AccessToken(new MicroSoft_AccessToken_RequestEntity()
                                        {
                                            code = code
                                        });

                                        //获取 user
                                        var userEntity = MicroSoft.User(new MicroSoft_User_RequestEntity()
                                        {
                                            access_token = tokenEntity.access_token
                                        });

                                        openId = userEntity.id.ToString();
                                        mo.OpenId5 = openId;

                                        mo.Nickname = userEntity.last_name + userEntity.first_name;
                                        mo.UserMail = userEntity.emails["account"].ToStringOrEmpty();
                                    }
                                    break;
                            }

                        }
                        catch (Exception ex)
                        {
                            vm.msg = ex.Message;
                        }
                    }

                    mo.UserCreateTime = DateTime.Now;
                    mo.UserName = openId;
                    mo.UserPwd = Core.CalcTo.MD5(openId);
                    if (!string.IsNullOrWhiteSpace(avatar))
                    {
                        mo.UserPhoto = Core.UniqueTo.LongId().ToString() + ".jpg";
                    }

                    if (string.IsNullOrWhiteSpace(openId))
                    {
                        vm.msg = "身份验证失败";
                    }
                    else
                    {
                        //判断是绑定操作
                        bool isbind = false;
                        if (User.Identity.IsAuthenticated)
                        {
                            try
                            {
                                var aboa = Request.Cookies["AccountBindOAuth"];

                                if (!string.IsNullOrWhiteSpace(aboa) && (DateTime.Now - DateTime.Parse(aboa)).TotalSeconds < 120)
                                {
                                    int uid = new Func.UserAuthAid(HttpContext).Get().UserId;

                                    using (var db = new ContextBase())
                                    {
                                        //检测是否绑定其它账号
                                        var queryIsBind = db.UserInfo.Where(x => x.UserId != uid);
                                        switch (vtype)
                                        {
                                            case ValidateloginType.qq:
                                                queryIsBind = queryIsBind.Where(x => x.OpenId1 == openId);
                                                break;
                                            case ValidateloginType.weibo:
                                                queryIsBind = queryIsBind.Where(x => x.OpenId2 == openId);
                                                break;
                                            case ValidateloginType.github:
                                                queryIsBind = queryIsBind.Where(x => x.OpenId3 == openId);
                                                break;
                                            case ValidateloginType.taobao:
                                                queryIsBind = queryIsBind.Where(x => x.OpenId4 == openId);
                                                break;
                                            case ValidateloginType.microsoft:
                                                queryIsBind = queryIsBind.Where(x => x.OpenId5 == openId);
                                                break;
                                        }
                                        if (queryIsBind.Count() > 0)
                                        {
                                            return Content("已绑定其它账号，不能重复绑定");
                                        }

                                        var userInfo = db.UserInfo.Find(uid);

                                        switch (vtype)
                                        {
                                            case ValidateloginType.qq:
                                                userInfo.OpenId1 = openId;
                                                break;
                                            case ValidateloginType.weibo:
                                                userInfo.OpenId2 = openId;
                                                break;
                                            case ValidateloginType.github:
                                                userInfo.OpenId3 = openId;
                                                break;
                                            case ValidateloginType.taobao:
                                                userInfo.OpenId4 = openId;
                                                break;
                                            case ValidateloginType.microsoft:
                                                userInfo.OpenId5 = openId;
                                                break;
                                        }
                                        db.UserInfo.Update(userInfo);
                                        db.SaveChanges();
                                    }

                                    Response.Cookies.Delete("AccountBindOAuth");
                                    isbind = true;
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }

                        //是绑定操作
                        if (isbind)
                        {
                            return Redirect("/user/setting");
                        }
                        else
                        {
                            using (var db = new ContextBase())
                            {
                                Domain.UserInfo vmo = null;
                                switch (vtype)
                                {
                                    case ValidateloginType.qq:
                                        vmo = db.UserInfo.FirstOrDefault(x => x.OpenId1 == openId);
                                        break;
                                    case ValidateloginType.weibo:
                                        vmo = db.UserInfo.FirstOrDefault(x => x.OpenId2 == openId);
                                        break;
                                    case ValidateloginType.github:
                                        vmo = db.UserInfo.FirstOrDefault(x => x.OpenId3 == openId);
                                        break;
                                    case ValidateloginType.taobao:
                                        vmo = db.UserInfo.FirstOrDefault(x => x.OpenId4 == openId);
                                        break;
                                    case ValidateloginType.microsoft:
                                        vmo = db.UserInfo.FirstOrDefault(x => x.OpenId5 == openId);
                                        break;
                                }
                                //未注册
                                if (vmo == null)
                                {
                                    var ruvm = RegisterUser(mo);
                                    if (ruvm.code == 200)
                                    {
                                        vm = ValidateLogin(vtype, mo);
                                        //拉取头像
                                        if (vm.code == 200 && (!string.IsNullOrWhiteSpace(avatar) || !string.IsNullOrWhiteSpace(avatarhd)))
                                        {
                                            try
                                            {
                                                System.Net.WebClient wc = new System.Net.WebClient();

                                                var rootdir = GlobalTo.WebRootPath + "/" + (GlobalTo.GetValue("StaticResource:RootDir").TrimStart('/').TrimEnd('/') + "/");
                                                var path = GlobalTo.GetValue("StaticResource:AvatarPath").TrimEnd('/').TrimStart('/') + '/';
                                                var fullpath = rootdir + path;

                                                if (!System.IO.Directory.Exists(fullpath))
                                                {
                                                    System.IO.Directory.CreateDirectory(fullpath);
                                                }
                                                if (!string.IsNullOrWhiteSpace(avatar))
                                                {
                                                    wc.DownloadFile(avatar, fullpath + mo.UserPhoto);
                                                }
                                                if (!string.IsNullOrWhiteSpace(avatarhd))
                                                {
                                                    wc.DownloadFile(avatarhd, fullpath + mo.UserPhoto.Replace(".jpg", "_lg.jpg"));
                                                }
                                            }
                                            catch (Exception)
                                            {
                                            }
                                        }
                                    }
                                    else
                                    {
                                        vm.msg = ruvm.msg;
                                    }
                                }
                                else
                                {
                                    vm = ValidateLogin(vtype, vmo);
                                }

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    vm.msg = ex.Message;
                }
            }

            //成功
            if (vm.code == 200)
            {
                return Redirect("/");
            }
            else
            {
                string msg = "【登录失败】（ " + vm.msg + " ）".ToEncode();
                return Redirect("/home/error?msg=" + msg);
            }
        }

        #endregion

        [Description("注销")]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var co = new CookieOptions();
            string cd = GlobalTo.GetValue("AuthCookieDomain");
            if (!string.IsNullOrWhiteSpace(cd))
            {
                co.Domain = cd;
            }
            HttpContext.Response.Cookies.Delete(CookieAuthenticationDefaults.AuthenticationScheme, co);

            var action = RouteData.Values["id"]?.ToString();
            return Redirect("/" + (action ?? ""));
        }
    }
}