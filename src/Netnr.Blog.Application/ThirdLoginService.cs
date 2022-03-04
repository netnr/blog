using Netnr.Login;

namespace Netnr.Blog.Application
{
    /// <summary>
    /// 第三方登录
    /// </summary>
    public class ThirdLoginService
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
                    case LoginBase.LoginType.WeChat:
                        {
                            var reqe = new WeChat_Authorization_RequestEntity();
                            if (!string.IsNullOrWhiteSpace(authType))
                            {
                                reqe.state = authType + reqe.state;
                            }
                            url = WeChat.AuthorizationHref(reqe);
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
                    case LoginBase.LoginType.Gitee:
                        {
                            var reqe = new Gitee_Authorize_RequestEntity();
                            if (!string.IsNullOrWhiteSpace(authType))
                            {
                                reqe.state = authType + reqe.state;
                            }
                            url = Gitee.AuthorizeHref(reqe);
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
                    case LoginBase.LoginType.Google:
                        {
                            var reqe = new Google_Authorize_RequestEntity();
                            if (!string.IsNullOrWhiteSpace(authType))
                            {
                                reqe.state = authType + reqe.state;
                            }
                            url = Google.AuthorizeHref(reqe);
                        }
                        break;
                    case LoginBase.LoginType.AliPay:
                        {
                            var reqe = new AliPay_Authorize_RequestEntity();
                            if (!string.IsNullOrWhiteSpace(authType))
                            {
                                reqe.state = authType + reqe.state;
                            }
                            url = AliPay.AuthorizeHref(reqe);
                        }
                        break;
                    case LoginBase.LoginType.StackOverflow:
                        {
                            var reqe = new StackOverflow_Authorize_RequestEntity();
                            if (!string.IsNullOrWhiteSpace(authType))
                            {
                                reqe.state = authType + reqe.state;
                            }
                            url = StackOverflow.AuthorizeHref(reqe);
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

        /// <summary>
        /// 获取快捷登录项
        /// </summary>
        /// <param name="umo">用户绑定状态</param>
        /// <returns></returns>
        public static List<ViewModel.QuickLoginVM> GetQuickLogin(Domain.UserInfo umo = null)
        {
            if (umo == null)
            {
                umo = new Domain.UserInfo();
            }

            return new List<ViewModel.QuickLoginVM>
            {
                new ViewModel.QuickLoginVM
                {
                    Key = "qq",
                    Name = "QQ",
                    Bind = !string.IsNullOrWhiteSpace(umo.OpenId1)
                },
                new ViewModel.QuickLoginVM
                {
                    Key = "weibo",
                    Name = "微博",
                    Bind = !string.IsNullOrWhiteSpace(umo.OpenId2)
                },
                new ViewModel.QuickLoginVM
                {
                    Key = "github",
                    Name = "GitHub",
                    Bind = !string.IsNullOrWhiteSpace(umo.OpenId3)
                },
                new ViewModel.QuickLoginVM
                {
                    Key = "taobao",
                    Name = "淘宝",
                    Bind = !string.IsNullOrWhiteSpace(umo.OpenId4)
                },
                new ViewModel.QuickLoginVM
                {
                    Key = "microsoft",
                    Name = "Microsoft",
                    Bind = !string.IsNullOrWhiteSpace(umo.OpenId5)
                },
                new ViewModel.QuickLoginVM
                {
                    Key = "dingtalk",
                    Name = "钉钉",
                    Bind = !string.IsNullOrWhiteSpace(umo.OpenId6)
                }
            };
        }
    }
}
