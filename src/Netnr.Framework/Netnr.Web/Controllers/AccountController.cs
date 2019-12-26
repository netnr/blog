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
using System.Collections.Generic;

namespace Netnr.Web.Controllers
{
    /// <summary>
    /// 账号
    /// </summary>
    public class AccountController : Controller
    {
        #region 注册

        /// <summary>
        /// 注册
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// 注册提交
        /// </summary>
        /// <param name="mo">账号、密码</param>
        /// <param name="RegisterCode">验证码</param>
        /// <returns></returns>
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
                if (Fast.ParsingTo.IsMail(mo.UserName))
                {
                    mo.UserMail = mo.UserName;
                }
                vm = RegisterUser(mo);
            }

            ViewData["UserName"] = mo.UserName;

            return View(vm);
        }

        /// <summary>
        /// 注册验证码
        /// </summary>
        /// <returns></returns>
        public FileResult RegisterCode()
        {
            //生成验证码
            string num = Core.RandomTo.NumCode(4);
            HttpContext.Session.SetString("RegisterCode", num);
            byte[] bytes = Fast.ImageTo.CreateImg(num);
            return File(bytes, "image/jpeg");
        }

        /// <summary>
        /// 公共注册
        /// </summary>
        /// <param name="mo">个人用户信息</param>
        /// <returns></returns>
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

        /// <summary>
        /// 登录页面
        /// </summary>
        /// <param name="ReturnUrl">登录跳转链接</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Login(string ReturnUrl)
        {
            //记录登录跳转
            Response.Cookies.Append("ReturnUrl", ReturnUrl ?? "");

            return View();
        }

        /// <summary>
        /// 登录验证
        /// </summary>
        /// <param name="mo">用户信息</param>
        /// <param name="remember">记住账号</param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult Login(Domain.UserInfo mo, int? remember)
        {
            var isRemember = remember == 1;
            var vm = ValidateLogin(ValidateloginType.local, mo, isRemember);
            if (vm.code == 200)
            {
                var rurl = Request.Cookies["ReturnUrl"];
                rurl = string.IsNullOrWhiteSpace(rurl) ? "/" : rurl;

                if (rurl.StartsWith("http"))
                {
                    rurl += "?cookie=ok";
                }

                return Redirect(rurl);
            }
            else
            {
                return View(vm);
            }
        }

        /// <summary>
        /// 第三方登录类型枚举
        /// </summary>
        public enum ValidateloginType
        {
            local, qq, weibo, github, taobao, microsoft, dingtalk
        }

        /// <summary>
        /// 公共登录验证
        /// </summary>
        /// <param name="vt">登录类型</param>
        /// <param name="mo">用户信息</param>
        /// <param name="isremember">记住账号</param>
        /// <returns></returns>
        private ActionResultVM ValidateLogin(ValidateloginType vt, Domain.UserInfo mo, bool isremember = true)
        {
            var vm = new ActionResultVM();

            string sql = string.Empty;

            using var db = new ContextBase();
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
                        if (Fast.ParsingTo.IsMail(mo.UserName))
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
                case ValidateloginType.dingtalk:
                    outMo = uiR.FirstOrDefault(x => x.OpenId6.Equals(mo.OpenId6));
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
                var usk = "UserSign_" + outMo.UserId;
                Core.CacheTo.Set(usk, outMo.UserSign, 5 * 60, false);

                //写入授权
                SetAuth(HttpContext, outMo, isremember);

                //生成Token
                vm.data = Func.UserAuthAid.TokenMake(outMo);

                vm.Set(ARTag.success);
            }
            catch (Exception ex)
            {
                vm.Set(ex);
            }

            return vm;
        }

        #endregion

        #region 登录（第三方）

        /// <summary>
        /// 第三方登录授权页面
        /// </summary>
        /// <returns></returns>
        public IActionResult Auth()
        {
            var authType = RouteData.Values["id"]?.ToString();
            var url = Func.UserAuthAid.ThirdLogin.LoginLink(authType);
            return Redirect(url);
        }

        /// <summary>
        /// 登录授权回调
        /// </summary>
        /// <param name="authorizeResult">获取授权码以及防伪标识</param>
        /// <returns></returns>
        public IActionResult AuthCallback(LoginBase.AuthorizeResult authorizeResult)
        {
            var vm = new ActionResultVM();

            try
            {
                if (string.IsNullOrWhiteSpace(authorizeResult.code))
                {
                    vm.Set(ARTag.unauthorized);
                }
                else
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

                    Enum.TryParse(RouteData.Values["id"]?.ToString(), true, out ValidateloginType vtype);

                    switch (vtype)
                    {
                        case ValidateloginType.qq:
                            {
                                //获取 access_token
                                var tokenEntity = QQ.AccessToken(new QQ_AccessToken_RequestEntity()
                                {
                                    code = authorizeResult.code
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
                                    code = authorizeResult.code
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
                                    code = authorizeResult.code
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
                                var tokenEntity = TaoBao.AccessToken(new TaoBao_AccessToken_RequestEntity()
                                {
                                    code = authorizeResult.code
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
                                    code = authorizeResult.code
                                });

                                //获取 user
                                var userEntity = MicroSoft.User(new MicroSoft_User_RequestEntity()
                                {
                                    access_token = tokenEntity.access_token
                                });

                                openId = userEntity.id.ToString();
                                mo.OpenId5 = openId;

                                mo.Nickname = userEntity.last_name + userEntity.first_name;
                                mo.UserMail = userEntity.emails?["account"].ToStringOrEmpty();
                            }
                            break;
                        case ValidateloginType.dingtalk:
                            {
                                //获取 user
                                var userEntity = DingTalk.User(new DingTalk_User_RequestEntity(), authorizeResult.code);

                                openId = userEntity.openid;
                                mo.OpenId6 = openId;

                                mo.Nickname = userEntity.nick;
                            }
                            break;
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
                        vm.Set(ARTag.unauthorized);
                        vm.msg = "身份验证失败";
                    }
                    else
                    {
                        //判断是绑定操作
                        bool isbind = User.Identity.IsAuthenticated && authorizeResult.state.StartsWith("bind");
                        if (isbind)
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
                                    case ValidateloginType.dingtalk:
                                        queryIsBind = queryIsBind.Where(x => x.OpenId6 == openId);
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
                                    case ValidateloginType.dingtalk:
                                        userInfo.OpenId6 = openId;
                                        break;
                                }
                                db.UserInfo.Update(userInfo);
                                db.SaveChanges();
                            }

                            return Redirect("/user/setting");
                        }
                        else
                        {
                            using var db = new ContextBase();
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
                                case ValidateloginType.dingtalk:
                                    vmo = db.UserInfo.FirstOrDefault(x => x.OpenId6 == openId);
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
                                            using var wc = new System.Net.WebClient();
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
                vm.Set(ex);

                Core.ConsoleTo.Log(ex);
            }

            //成功
            if (vm.code == 200)
            {
                var rurl = Request.Cookies["ReturnUrl"];
                rurl = string.IsNullOrWhiteSpace(rurl) ? "/" : rurl;

                if (rurl.StartsWith("http"))
                {
                    rurl += "?cookie=ok";
                }

                return Redirect(rurl);
            }
            else
            {
                string msg = "【登录失败】（ " + vm.msg + " ）".ToEncode();
                return Redirect("/home/error?msg=" + msg);
            }
        }

        #endregion

        /// <summary>
        /// 注销
        /// </summary>
        /// <returns></returns>
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();
            var action = RouteData.Values["id"]?.ToString();
            return Redirect("/" + (action ?? ""));
        }

        /// <summary>
        /// 写入授权
        /// </summary>
        /// <param name="context">当前上下文</param>
        /// <param name="user">用户信息</param>
        /// <param name="isremember">是否记住账号</param>
        public void SetAuth(HttpContext context, Domain.UserInfo user, bool isremember = true)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.PrimarySid, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.GivenName, user.Nickname ?? ""),
                new Claim(ClaimTypes.Sid, user.UserSign),
                new Claim(ClaimTypes.UserData, user.UserPhoto ?? ""),
            };

            var cp = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

            var authProperties = new AuthenticationProperties();
            if (isremember)
            {
                //记住
                authProperties.IsPersistent = true;
                authProperties.ExpiresUtc = DateTimeOffset.Now.AddDays(10);
            }

            //写入授权
            context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, cp, authProperties).Wait();
        }
    }
}