using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Netnr.Blog.Data;
using Netnr.Login;
using System.Security.Claims;
using Netnr.Core;
using Netnr.SharedFast;
using Netnr.SharedDrawing;

namespace Netnr.Blog.Web.Controllers
{
    /// <summary>
    /// 账号
    /// </summary>
    public class AccountController : Controller
    {
        public ContextBase db;

        public AccountController(ContextBase cb)
        {
            db = cb;
        }

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
            var vm = new SharedResultVM();
            if (string.IsNullOrWhiteSpace(RegisterCode) || HttpContext.Session.GetString("RegisterCode") != RegisterCode)
            {
                vm.Msg = "验证码错误或已过期";
            }
            else if (!(mo.UserName?.Length >= 5 && mo.UserPwd?.Length >= 5))
            {
                vm.Msg = "账号、密码长度至少 5 位数";
            }
            else
            {
                mo.UserPwd = CalcTo.MD5(mo.UserPwd);
                mo.UserCreateTime = DateTime.Now;

                //邮箱注册
                if (ParsingTo.IsMail(mo.UserName))
                {
                    mo.UserMail = mo.UserName;
                }
                vm = RegisterUser(mo);
            }

            ViewData["UserName"] = mo.UserName;

            HttpContext.Session.Remove("RegisterCode");
            return View(vm);
        }

        /// <summary>
        /// 注册验证码
        /// </summary>
        /// <returns></returns>
        public FileResult RegisterCode()
        {
            //生成验证码
            string num = RandomTo.NumCode(4);
            HttpContext.Session.SetString("RegisterCode", num);
            byte[] bytes = ImageTo.Captcha(num);
            return File(bytes, "image/jpeg");
        }

        /// <summary>
        /// 公共注册
        /// </summary>
        /// <param name="mo">个人用户信息</param>
        /// <returns></returns>
        private SharedResultVM RegisterUser(Domain.UserInfo mo)
        {
            var vm = new SharedResultVM();

            var isok = true;

            //邮箱注册
            if (!string.IsNullOrWhiteSpace(mo.UserMail))
            {
                isok = !db.UserInfo.Any(x => x.UserName == mo.UserName || x.UserMail == mo.UserMail);

                vm.Set(SharedEnum.RTag.exist);
                vm.Msg = "该邮箱已经注册";
            }
            else
            {
                isok = !db.UserInfo.Any(x => x.UserName == mo.UserName);

                vm.Set(SharedEnum.RTag.exist);
                vm.Msg = "该账号已经注册";
            }

            if (isok)
            {
                db.UserInfo.Add(mo);
                int num = db.SaveChanges();
                vm.Set(num > 0);
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
            var vm = ValidateLogin(null, mo, isRemember);
            if (vm.Code == 200)
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
        /// 公共登录验证
        /// </summary>
        /// <param name="vt">登录类型</param>
        /// <param name="mo">用户信息</param>
        /// <param name="isremember">记住账号</param>
        /// <returns></returns>
        private SharedResultVM ValidateLogin(LoginBase.LoginType? vt, Domain.UserInfo mo, bool isremember = true)
        {
            var vm = new SharedResultVM();

            string sql = string.Empty;

            var uiR = db.UserInfo;
            Domain.UserInfo outMo = new();

            switch (vt)
            {
                case LoginBase.LoginType.QQ:
                    outMo = uiR.FirstOrDefault(x => x.OpenId1.Equals(mo.OpenId1));
                    break;
                case LoginBase.LoginType.WeiBo:
                    outMo = uiR.FirstOrDefault(x => x.OpenId2.Equals(mo.OpenId2));
                    break;
                case LoginBase.LoginType.GitHub:
                    outMo = uiR.FirstOrDefault(x => x.OpenId3.Equals(mo.OpenId3));
                    break;
                case LoginBase.LoginType.TaoBao:
                    outMo = uiR.FirstOrDefault(x => x.OpenId4.Equals(mo.OpenId4));
                    break;
                case LoginBase.LoginType.MicroSoft:
                    outMo = uiR.FirstOrDefault(x => x.OpenId5.Equals(mo.OpenId5));
                    break;
                case LoginBase.LoginType.DingTalk:
                    outMo = uiR.FirstOrDefault(x => x.OpenId6.Equals(mo.OpenId6));
                    break;
                default:
                    if (string.IsNullOrWhiteSpace(mo.UserName) || string.IsNullOrWhiteSpace(mo.UserPwd))
                    {
                        vm.Msg = "用户名或密码不能为空";
                        return vm;
                    }
                    else
                    {
                        mo.UserPwd = CalcTo.MD5(mo.UserPwd);

                        //邮箱登录
                        if (ParsingTo.IsMail(mo.UserName))
                        {
                            outMo = uiR.FirstOrDefault(x => x.UserMail == mo.UserName && x.UserPwd == mo.UserPwd);
                        }
                        else
                        {
                            outMo = uiR.FirstOrDefault(x => x.UserName == mo.UserName && x.UserPwd == mo.UserPwd);
                        }
                    }
                    break;
            }

            if (outMo == null || outMo.UserId == 0)
            {
                vm.Msg = "用户名或密码错误";
                return vm;
            }

            if (outMo.LoginLimit == 1)
            {
                vm.Msg = "用户已被禁止登录";
                return vm;
            }

            try
            {
                //刷新登录标记
                outMo.UserLoginTime = DateTime.Now;
                outMo.UserSign = outMo.UserLoginTime.Value.ToTimestamp().ToString();
                uiR.Update(outMo);
                db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            try
            {
                //登录标记 缓存5分钟，绝对过期
                if (GlobalTo.GetValue<bool>("Common:SingleSignOn"))
                {
                    var usk = "UserSign_" + outMo.UserId;
                    CacheTo.Set(usk, outMo.UserSign, 5 * 60, false);
                }

                //写入授权
                SetAuth(HttpContext, outMo, isremember);

                //生成Token
                vm.Data = Apps.LoginService.TokenMake(outMo);

                vm.Set(SharedEnum.RTag.success);
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
            var url = Application.ThirdLoginService.LoginLink(authType);
            return Redirect(url);
        }

        /// <summary>
        /// 登录授权回调
        /// </summary>
        /// <param name="authorizeResult">获取授权码以及防伪标识</param>
        /// <returns></returns>
        public IActionResult AuthCallback(LoginBase.AuthorizeResult authorizeResult)
        {
            var vm = new SharedResultVM();

            try
            {
                if (string.IsNullOrWhiteSpace(authorizeResult.code))
                {
                    vm.Set(SharedEnum.RTag.unauthorized);
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

                    Enum.TryParse(RouteData.Values["id"]?.ToString(), true, out LoginBase.LoginType vtype);

                    switch (vtype)
                    {
                        case LoginBase.LoginType.QQ:
                            {
                                //获取 access_token
                                var tokenEntity = QQ.AccessToken(new QQ_AccessToken_RequestEntity()
                                {
                                    code = authorizeResult.code
                                });
                                Console.WriteLine(tokenEntity.ToJson());

                                //获取 OpendId
                                var openidEntity = QQ.OpenId(tokenEntity.access_token);
                                Console.WriteLine(openidEntity.ToJson());

                                //获取 UserInfo
                                var userEntity = QQ.OpenId_Get_User_Info(new QQ_OpenAPI_RequestEntity()
                                {
                                    access_token = tokenEntity.access_token,
                                    openid = openidEntity.openid
                                });
                                Console.WriteLine(userEntity.ToJson());

                                //身份唯一标识
                                openId = openidEntity.openid;
                                mo.OpenId1 = openId;

                                mo.Nickname = userEntity.nickname;
                                mo.UserSex = userEntity.gender == "男" ? 1 : 2;
                                mo.UserSay = "";
                                mo.UserUrl = "";

                                avatar = userEntity.figureurl_2;
                            }
                            break;
                        case LoginBase.LoginType.WeiBo:
                            {
                                //获取 access_token
                                var tokenEntity = Weibo.AccessToken(new Weibo_AccessToken_RequestEntity()
                                {
                                    code = authorizeResult.code
                                });
                                Console.WriteLine(tokenEntity.ToJson());

                                //获取 access_token 的授权信息
                                var tokenInfoEntity = Weibo.GetTokenInfo(tokenEntity.access_token);
                                Console.WriteLine(tokenInfoEntity.ToJson());

                                //获取 users/show
                                var userEntity = Weibo.UserShow(new Weibo_UserShow_RequestEntity()
                                {
                                    access_token = tokenEntity.access_token,
                                    uid = Convert.ToInt64(tokenInfoEntity.uid)
                                });
                                Console.WriteLine(userEntity.ToJson());

                                openId = tokenEntity.access_token;
                                mo.OpenId2 = openId;

                                mo.Nickname = userEntity.screen_name;
                                mo.UserSex = userEntity.gender == "m" ? 1 : userEntity.gender == "f" ? 2 : 0;
                                mo.UserSay = userEntity.description;
                                mo.UserUrl = userEntity.domain;

                                avatar = userEntity.avatar_large;
                            }
                            break;
                        case LoginBase.LoginType.WeChat:
                            {
                                //获取 access_token
                                var tokenEntity = Netnr.Login.WeChat.AccessToken(new WeChat_AccessToken_RequestEntity()
                                {
                                    code = authorizeResult.code
                                });
                                Console.WriteLine(tokenEntity.ToJson());

                                //openId = tokenEntity.openid;

                                //获取 user
                                var userEntity = Netnr.Login.WeChat.Get_User_Info(new WeChat_OpenAPI_RequestEntity()
                                {
                                    access_token = tokenEntity.access_token,
                                    openid = tokenEntity.openid
                                });
                                Console.WriteLine(userEntity.ToJson());

                                avatar = userEntity.headimgurl;
                            }
                            break;
                        case LoginBase.LoginType.GitHub:
                            {
                                //获取 access_token
                                var tokenEntity = GitHub.AccessToken(new GitHub_AccessToken_RequestEntity()
                                {
                                    code = authorizeResult.code
                                });
                                Console.WriteLine(tokenEntity.ToJson());

                                //获取 user
                                var userEntity = GitHub.User(tokenEntity.access_token);
                                Console.WriteLine(userEntity.ToJson());

                                openId = userEntity.id.ToString();
                                mo.OpenId3 = openId;

                                mo.Nickname = userEntity.name;
                                mo.UserSay = userEntity.bio;
                                mo.UserUrl = userEntity.blog;
                                mo.UserMail = userEntity.email;

                                avatar = userEntity.avatar_url;
                            }
                            break;
                        case LoginBase.LoginType.Gitee:
                            {
                                //获取 access_token
                                var tokenEntity = Gitee.AccessToken(new Gitee_AccessToken_RequestEntity()
                                {
                                    code = authorizeResult.code
                                });
                                Console.WriteLine(tokenEntity.ToJson());

                                //获取 user
                                var userEntity = Gitee.User(tokenEntity.access_token);
                                Console.WriteLine(userEntity.ToJson());

                                //openId = userEntity.id.ToString();

                                mo.Nickname = userEntity.name;
                                mo.UserSay = userEntity.bio;
                                mo.UserUrl = userEntity.blog;

                                avatar = userEntity.avatar_url;
                            }
                            break;
                        case LoginBase.LoginType.TaoBao:
                            {
                                //获取 access_token
                                var tokenEntity = TaoBao.AccessToken(new TaoBao_AccessToken_RequestEntity()
                                {
                                    code = authorizeResult.code
                                });
                                Console.WriteLine(tokenEntity.ToJson());

                                openId = tokenEntity.open_uid;
                                mo.OpenId4 = openId;

                                mo.Nickname = "淘宝用户";
                            }
                            break;
                        case LoginBase.LoginType.MicroSoft:
                            {
                                //获取 access_token
                                var tokenEntity = MicroSoft.AccessToken(new MicroSoft_AccessToken_RequestEntity()
                                {
                                    code = authorizeResult.code
                                });
                                Console.WriteLine(tokenEntity.ToJson());

                                //获取 user
                                var userEntity = MicroSoft.User(tokenEntity.access_token);
                                Console.WriteLine(userEntity.ToJson());

                                openId = userEntity.id;
                                mo.OpenId5 = openId;

                                mo.Nickname = userEntity.last_name + userEntity.first_name;
                                mo.UserMail = userEntity.emails?["account"].ToStringOrEmpty();
                            }
                            break;
                        case LoginBase.LoginType.DingTalk:
                            {
                                //获取 user
                                var userEntity = DingTalk.User(new DingTalk_User_RequestEntity(), authorizeResult.code);
                                Console.WriteLine(userEntity.ToJson());

                                openId = userEntity.openid;
                                mo.OpenId6 = openId;

                                mo.Nickname = userEntity.nick;
                            }
                            break;
                        case LoginBase.LoginType.Google:
                            {
                                //获取 access_token
                                var tokenEntity = Google.AccessToken(new Google_AccessToken_RequestEntity()
                                {
                                    code = authorizeResult.code
                                });
                                Console.WriteLine(tokenEntity.ToJson());

                                //获取 user
                                var userEntity = Google.User(tokenEntity.access_token);
                                Console.WriteLine(userEntity.ToJson());

                                //openId = userEntity.sub;

                                avatar = userEntity.picture;
                            }
                            break;
                        case LoginBase.LoginType.AliPay:
                            {
                                //获取 access_token
                                var tokenEntity = AliPay.AccessToken(new AliPay_AccessToken_RequestEntity()
                                {
                                    code = authorizeResult.code
                                });
                                Console.WriteLine(tokenEntity.ToJson());

                                //openId = tokenEntity.user_id;

                                //获取 user
                                var userEntity = AliPay.User(new AliPay_User_RequestEntity()
                                {
                                    auth_token = tokenEntity.access_token
                                });
                                Console.WriteLine(userEntity.ToJson());

                                avatar = userEntity.avatar;
                            }
                            break;
                        case LoginBase.LoginType.StackOverflow:
                            {
                                //获取 access_token
                                var tokenEntity = StackOverflow.AccessToken(new StackOverflow_AccessToken_RequestEntity()
                                {
                                    code = authorizeResult.code
                                });
                                Console.WriteLine(tokenEntity.ToJson());

                                //获取 user
                                var userEntity = StackOverflow.User(new StackOverflow_User_RequestEntity()
                                {
                                    access_token = tokenEntity.access_token
                                });
                                Console.WriteLine(userEntity.ToJson());

                                //openId= userEntity.user_id;

                                avatar = userEntity.profile_image;
                            }
                            break;
                    }

                    mo.UserCreateTime = DateTime.Now;
                    mo.UserName = openId;
                    mo.UserPwd = CalcTo.MD5(openId);
                    if (!string.IsNullOrWhiteSpace(avatar))
                    {
                        mo.UserPhoto = UniqueTo.LongId().ToString() + ".jpg";
                    }
                    Console.WriteLine(mo.ToJson());

                    if (string.IsNullOrWhiteSpace(openId))
                    {
                        vm.Set(SharedEnum.RTag.unauthorized);
                        vm.Msg = "身份验证失败";
                    }
                    else
                    {
                        //判断是绑定操作
                        bool isbind = User.Identity.IsAuthenticated && authorizeResult.state.StartsWith("bind");
                        if (isbind)
                        {
                            int uid = Apps.LoginService.Get(HttpContext).UserId;

                            //检测是否绑定其它账号
                            var queryIsBind = db.UserInfo.Where(x => x.UserId != uid);
                            switch (vtype)
                            {
                                case LoginBase.LoginType.QQ:
                                    queryIsBind = queryIsBind.Where(x => x.OpenId1 == openId);
                                    break;
                                case LoginBase.LoginType.WeiBo:
                                    queryIsBind = queryIsBind.Where(x => x.OpenId2 == openId);
                                    break;
                                case LoginBase.LoginType.GitHub:
                                    queryIsBind = queryIsBind.Where(x => x.OpenId3 == openId);
                                    break;
                                case LoginBase.LoginType.TaoBao:
                                    queryIsBind = queryIsBind.Where(x => x.OpenId4 == openId);
                                    break;
                                case LoginBase.LoginType.MicroSoft:
                                    queryIsBind = queryIsBind.Where(x => x.OpenId5 == openId);
                                    break;
                                case LoginBase.LoginType.DingTalk:
                                    queryIsBind = queryIsBind.Where(x => x.OpenId6 == openId);
                                    break;
                            }
                            if (queryIsBind.Any())
                            {
                                return Content("已绑定其它账号，不能重复绑定");
                            }

                            var userInfo = db.UserInfo.Find(uid);

                            switch (vtype)
                            {
                                case LoginBase.LoginType.QQ:
                                    userInfo.OpenId1 = openId;
                                    break;
                                case LoginBase.LoginType.WeiBo:
                                    userInfo.OpenId2 = openId;
                                    break;
                                case LoginBase.LoginType.GitHub:
                                    userInfo.OpenId3 = openId;
                                    break;
                                case LoginBase.LoginType.TaoBao:
                                    userInfo.OpenId4 = openId;
                                    break;
                                case LoginBase.LoginType.MicroSoft:
                                    userInfo.OpenId5 = openId;
                                    break;
                                case LoginBase.LoginType.DingTalk:
                                    userInfo.OpenId6 = openId;
                                    break;
                            }
                            db.UserInfo.Update(userInfo);
                            db.SaveChanges();

                            return Redirect("/user/setting");
                        }
                        else
                        {
                            Domain.UserInfo vmo = null;
                            switch (vtype)
                            {
                                case LoginBase.LoginType.QQ:
                                    vmo = db.UserInfo.FirstOrDefault(x => x.OpenId1 == openId);
                                    break;
                                case LoginBase.LoginType.WeiBo:
                                    vmo = db.UserInfo.FirstOrDefault(x => x.OpenId2 == openId);
                                    break;
                                case LoginBase.LoginType.GitHub:
                                    vmo = db.UserInfo.FirstOrDefault(x => x.OpenId3 == openId);
                                    break;
                                case LoginBase.LoginType.TaoBao:
                                    vmo = db.UserInfo.FirstOrDefault(x => x.OpenId4 == openId);
                                    break;
                                case LoginBase.LoginType.MicroSoft:
                                    vmo = db.UserInfo.FirstOrDefault(x => x.OpenId5 == openId);
                                    break;
                                case LoginBase.LoginType.DingTalk:
                                    vmo = db.UserInfo.FirstOrDefault(x => x.OpenId6 == openId);
                                    break;
                            }
                            //未注册
                            if (vmo == null)
                            {
                                var ruvm = RegisterUser(mo);
                                if (ruvm.Code == 200)
                                {
                                    vm = ValidateLogin(vtype, mo);
                                    //拉取头像
                                    if (vm.Code == 200 && !string.IsNullOrWhiteSpace(avatar))
                                    {
                                        try
                                        {
                                            //物理根路径
                                            var prp = GlobalTo.GetValue("StaticResource:PhysicalRootPath").Replace("~", GlobalTo.ContentRootPath);
                                            var ppath = PathTo.Combine(prp, GlobalTo.GetValue("StaticResource:AvatarPath"));

                                            if (!Directory.Exists(ppath))
                                            {
                                                Directory.CreateDirectory(ppath);
                                            }

                                            HttpTo.DownloadSave(HttpTo.HWRequest(avatar), PathTo.Combine(ppath, mo.UserPhoto));
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine(ex);
                                        }
                                    }
                                }
                                else
                                {
                                    vm.Msg = ruvm.Msg;
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
                Apps.FilterConfigs.WriteLog(HttpContext, ex);
                Response.Headers["X-Output-Msg"] = ex.ToJson();
                vm.Set(ex);
            }

            //成功
            if (vm.Code == 200)
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
                return Redirect("/home/error");
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