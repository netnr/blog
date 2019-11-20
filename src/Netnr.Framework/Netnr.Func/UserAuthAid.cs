using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Netnr.Login;

namespace Netnr.Func
{
    /// <summary>
    /// 用户授权
    /// </summary>
    public class UserAuthAid
    {
        private readonly HttpContext context;

        /// <summary>
        /// 构造，拿到当前上下文
        /// </summary>
        /// <param name="httpContext"></param>
        public UserAuthAid(HttpContext httpContext)
        {
            context = httpContext;
        }

        /// <summary>
        /// 获取授权用户
        /// </summary>
        /// <returns></returns>
        public Domain.UserInfo Get()
        {
            var user = context.User;

            var usermo = new Domain.UserInfo
            {
                UserId = Convert.ToInt32(user.FindFirst(ClaimTypes.PrimarySid)?.Value),
                UserName = user.FindFirst(ClaimTypes.Name)?.Value,
                Nickname = user.FindFirst(ClaimTypes.GivenName)?.Value,
                UserSign = user.FindFirst(ClaimTypes.Sid)?.Value,
                UserPhoto = user.FindFirst(ClaimTypes.UserData)?.Value
            };

            return usermo;
        }

        /// <summary>
        /// 第三方登录
        /// </summary>
        public class ThirdLogin
        {
            /// <summary>
            /// 登录链接
            /// </summary>
            /// <param name="loginType">登录类型</param>
            /// <param name="authType">登录防伪追加标识，区分登录、注册</param>
            /// <returns></returns>
            public static string LoginLink(string loginType, string authType = "")
            {
                string url = string.Empty;

                if (Enum.TryParse(loginType, true, out LoginBase.LoginType vtype))
                {
                    switch (vtype)
                    {
                        case LoginBase.LoginType.QQ:
                            {
                                var reqe = new QQ_Authorization_RequestEntity();
                                if (!string.IsNullOrWhiteSpace(authType))
                                {
                                    reqe.state = authType + reqe.state;
                                }
                                url = QQ.AuthorizationHref(reqe);
                            }
                            break;
                        case LoginBase.LoginType.WeiBo:
                            {
                                var reqe = new Weibo_Authorize_RequestEntity();
                                if (!string.IsNullOrWhiteSpace(authType))
                                {
                                    reqe.state = authType + reqe.state;
                                }
                                url = Weibo.AuthorizeHref(reqe);
                            }
                            break;
                        case LoginBase.LoginType.GitHub:
                            {
                                var reqe = new GitHub_Authorize_RequestEntity();
                                if (!string.IsNullOrWhiteSpace(authType))
                                {
                                    reqe.state = authType + reqe.state;
                                }
                                url = GitHub.AuthorizeHref(reqe);
                            }
                            break;
                        case LoginBase.LoginType.TaoBao:
                            {
                                var reqe = new TaoBao_Authorize_RequestEntity();
                                if (!string.IsNullOrWhiteSpace(authType))
                                {
                                    reqe.state = authType + reqe.state;
                                }
                                url = TaoBao.AuthorizeHref(reqe);
                            }
                            break;
                        case LoginBase.LoginType.MicroSoft:
                            {
                                var reqe = new MicroSoft_Authorize_RequestEntity();
                                if (!string.IsNullOrWhiteSpace(authType))
                                {
                                    reqe.state = authType + reqe.state;
                                }
                                url = MicroSoft.AuthorizeHref(reqe);
                            }
                            break;
                        case LoginBase.LoginType.DingTalk:
                            {
                                var reqe = new DingTalk_Authorize_RequestEntity();
                                if (!string.IsNullOrWhiteSpace(authType))
                                {
                                    reqe.state = authType + reqe.state;
                                }
                                url = DingTalk.AuthorizeHref_ScanCode(reqe);
                            }
                            break;
                    }
                }

                if (string.IsNullOrWhiteSpace(url))
                {
                    url = "/account/login";
                }

                return url;
            }
        }
    }
}
