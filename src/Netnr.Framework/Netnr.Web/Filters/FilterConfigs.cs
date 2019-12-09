using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Netnr.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Xml;

namespace Netnr.Web.Filters
{
    /// <summary>
    /// 过滤器
    /// </summary>
    public class FilterConfigs
    {
        /// <summary>
        /// 全局错误处理
        /// </summary>
        public class ErrorActionFilter : IExceptionFilter
        {
            public void OnException(ExceptionContext context)
            {
                Core.ConsoleTo.Log(context.Exception);
            }
        }

        private static Dictionary<string, string> _dicDescription;

        /// <summary>
        /// 根据生成的注释文件XML获取Action的注释
        /// </summary>
        public static Dictionary<string, string> DicDescription
        {
            get
            {
                if (_dicDescription == null)
                {
                    var ass = System.Reflection.Assembly.GetExecutingAssembly();
                    var listController = ass.ExportedTypes.Where(x => x.BaseType.FullName == "Microsoft.AspNetCore.Mvc.Controller").ToList();

                    //载入xml注释
                    var cp = AppContext.BaseDirectory + ass.FullName.Split(',').FirstOrDefault() + ".xml";
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(cp);
                    var xns = xmldoc.DocumentElement.SelectSingleNode("members").SelectNodes("member");
                    var listMember = new List<XmlNode>();
                    for (int i = 0; i < xns.Count; i++)
                    {
                        listMember.Add(xns[i]);
                    }

                    var dic = new Dictionary<string, string>();
                    foreach (var conll in listController)
                    {
                        var methods = conll.GetMethods();
                        foreach (var item in methods)
                        {
                            if (item.DeclaringType == conll)
                            {
                                string remark = "未备注说明";

                                //方法完整命名空间及名称
                                var cname = "M:" + conll.FullName + "." + item.Name;
                                //方法参数
                                var cparam = item.GetParameters();
                                if (cparam.Length > 0)
                                {
                                    var listParam = new List<string>();
                                    foreach (var par in cparam)
                                    {
                                        listParam.Add(par.ParameterType.FullName);
                                    }
                                    cname += "(" + string.Join(",", listParam) + ")";
                                }

                                var xnm = listMember.FirstOrDefault(x => x.Attributes["name"].Value.ToString() == cname);
                                if (xnm != null)
                                {
                                    remark = xnm.SelectSingleNode("summary").InnerText.ToString().Trim();
                                }

                                var action = (conll.Name.Replace("Controller", "/") + item.Name).ToLower();
                                if (!dic.ContainsKey(action))
                                {
                                    dic.Add(action, remark);
                                }
                            }
                        }
                    }
                    _dicDescription = dic;
                }

                return _dicDescription;
            }
            set
            {
                _dicDescription = value;
            }
        }

        /// <summary>
        /// 全局过滤器
        /// </summary>
        public class GlobalFilter : ActionFilterAttribute
        {
            public override void OnActionExecuting(ActionExecutingContext context)
            {
                //日志
                try
                {
                    var hc = context.HttpContext;

                    string controller = context.RouteData.Values["controller"].ToString().ToLower();
                    string action = context.RouteData.Values["action"].ToString().ToLower();
                    string url = hc.Request.Path.ToString() + hc.Request.QueryString.Value;
                    var referer = hc.Request.Headers["referer"].ToString();
                    var requestid = Core.UniqueTo.LongId().ToString();
                    hc.Response.Headers.Add("X-Request-Id", requestid);

                    //客户端信息
                    var ct = new Fast.ClientTo(hc);

                    //用户信息
                    var userinfo = new Func.UserAuthAid(hc).Get();

                    //日志保存
                    var mo = new Func.LogsAid.LogsVM()
                    {
                        LogName = userinfo?.UserName,
                        LogNickname = userinfo?.Nickname,
                        LogRequestId = requestid,
                        LogAction = controller + "/" + action,
                        LogUrl = url,
                        LogIp = ct.IPv4.Split(',')[0].Trim(),
                        LogReferer = referer,
                        LogCreateTime = DateTime.Now,
                        LogBrowserName = ct.BrowserName,
                        LogSystemName = ct.SystemName,
                        LogGroup = 1,
                        LogLevel = "info"
                    };
                    mo.LogContent = DicDescription[mo.LogAction.ToLower()];

                    Func.LogsAid.Insert(mo);
                }
                catch (Exception)
                {
                }

                base.OnActionExecuting(context);
            }
        }

        /// <summary>
        /// 需要授权访问
        /// </summary>
        public class LogonSignValid : IAuthorizationFilter
        {
            public void OnAuthorization(AuthorizationFilterContext context)
            {
                //验证登录标记是最新，不是则注销登录（即同一用户不允许同时在线，按缓存时间生效）
                if (context.HttpContext.User.Identity.IsAuthenticated)
                {
                    var uinfo = new Func.UserAuthAid(context.HttpContext).Get();

                    string ServerSign = HelpFuncTo.GetLogonSign(uinfo.UserId);
                    if (uinfo.UserSign != ServerSign)
                    {
                        context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    }
                }
            }
        }

        /// <summary>
        /// 允许跨域
        /// </summary>
        public class AllowCors : Attribute, IActionFilter
        {
            public void OnActionExecuted(ActionExecutedContext context)
            {

            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                var res = context.HttpContext.Response;

                var origin = context.HttpContext.Request.Headers["Origin"].ToString();

                var dicAk = new Dictionary<string, string>
                {
                    { "Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS" },
                    { "Access-Control-Allow-Headers", "Accept, Authorization, Cache-Control, Content-Type, DNT, If-Modified-Since, Keep-Alive, Origin, User-Agent, X-Requested-With, Token, x-access-token" }
                };

                if (string.IsNullOrWhiteSpace(origin))
                {
                    dicAk.Add("Access-Control-Allow-Origin", "*");
                }
                else
                {
                    dicAk.Add("Access-Control-Allow-Origin", origin);
                    dicAk.Add("Access-Control-Allow-Credentials", "true");
                }

                foreach (var ak in dicAk.Keys)
                {
                    res.Headers.Remove(ak);
                    res.Headers.Add(ak, dicAk[ak]);
                }

                if (context.HttpContext.Request.Method == "OPTIONS")
                {
                    context.Result = new OkResult();
                }
            }
        }

        /// <summary>
        /// 是管理员
        /// </summary>
        public class IsAdmin : Attribute, IActionFilter
        {
            public void OnActionExecuted(ActionExecutedContext context)
            {

            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                bool isv = false;

                if (context.HttpContext.User.Identity.IsAuthenticated)
                {
                    var uinfo = new Func.UserAuthAid(context.HttpContext).Get();
                    isv = uinfo.UserId == GlobalTo.GetValue<int>("AdminId");
                }

                if (!isv)
                {
                    context.Result = new ContentResult()
                    {
                        Content = "unauthorized",
                        StatusCode = 401
                    };
                }
            }
        }

        /// <summary>
        /// 有效验证（邮箱）
        /// </summary>
        public class IsValidMail : Attribute, IActionFilter
        {
            public void OnActionExecuted(ActionExecutedContext context)
            {

            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                bool isv = false;

                if (context.HttpContext.User.Identity.IsAuthenticated)
                {
                    var uinfo = new Func.UserAuthAid(context.HttpContext).Get();

                    //已验证邮箱
                    using var db = new ContextBase();
                    uinfo = db.UserInfo.Find(uinfo.UserId);
                    if (uinfo.UserId == 1 || uinfo.UserMailValid == 1)
                    {
                        isv = true;
                    }
                }

                if (!isv)
                {
                    var url = "/home/valid";
                    context.Result = new RedirectResult(url);
                }
            }
        }

        /// <summary>
        /// 有效授权（Cookie、Token）
        /// </summary>
        public class IsValidAuth : Attribute, IActionFilter
        {
            public void OnActionExecuted(ActionExecutedContext context)
            {

            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                var mo = new Func.UserAuthAid(context.HttpContext).Get();

                if (mo.UserId == 0)
                {
                    context.Result = new ContentResult()
                    {
                        Content = "unauthorized",
                        StatusCode = 401
                    };
                }
            }
        }

        /// <summary>
        /// LocalAuth 过滤，本地授权
        /// </summary>
        public class LocalAuth : Attribute, IActionFilter
        {
            public void OnActionExecuted(ActionExecutedContext context)
            {

            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                var sk = context.HttpContext.Request.Cookies["sk"] ?? "";
                bool br = HelpFuncTo.LocalIsAuth(sk);

                if (!br)
                {
                    var url = "/home/auth?returnUrl=" + System.Web.HttpUtility.UrlEncode(context.HttpContext.Request.Path);
                    context.Result = new RedirectResult(url);
                }
            }
        }

        #region 辅助方法
        public class HelpFuncTo
        {
            /// <summary>
            /// 本地授权：被减数与当前小时、分钟分别求差，满足容错分钟数即有效的KEY
            /// 
            /// 举例：
            /// 配置 小时被减数80，分钟被减数80，假定现在时间是 16:50
            /// 那么对应的KEY是 80-16=64,80-50=30 即 6430
            /// 
            /// </summary>
            /// <param name="sk"></param>
            /// <returns></returns>
            public static bool LocalIsAuth(string sk)
            {
                bool b = false;
                if (sk.Length == 4)
                {
                    try
                    {
                        //小时被减数，24小时制
                        int bh = GlobalTo.GetValue<int>("LocalAuth:BeHour");
                        //分钟被减数
                        int bm = GlobalTo.GetValue<int>("LocalAuth:BeMinute");
                        //容错分钟数
                        int rm = GlobalTo.GetValue<int>("LocalAuth:RangeMinute");

                        string h = (bh - Convert.ToInt32(sk.Substring(0, 2))).ToString().PadLeft(2, '0');

                        int mm = bm - Convert.ToInt32(sk.Substring(2));
                        string m = (Math.Min(mm, 59)).ToString().PadLeft(2, '0');

                        DateTime dtn = DateTime.Now;
                        if (DateTime.TryParse(dtn.ToString("yyyy-MM-dd ") + h + ":" + m, out DateTime dta))
                        {
                            //与当前时间容错分钟
                            if (dta >= dtn.AddMinutes(-rm) && dta <= dtn.AddMinutes(rm))
                            {
                                b = true;
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
                return b;
            }

            /// <summary>
            /// 获取最新登录标记，用于对比本地，踢出下线
            /// </summary>
            /// <param name="UserId">登录的UserId</param>
            /// <param name="Cache">优先取缓存</param>
            /// <returns></returns>
            public static string GetLogonSign(int UserId, bool Cache = true)
            {
                string result = string.Empty;
                var usk = "UserSign_" + UserId;
                var us = Core.CacheTo.Get(usk) as string;
                if (Cache && !string.IsNullOrEmpty(us))
                {
                    result = us;
                }
                else
                {
                    using var db = new ContextBase();
                    var uiMo = db.UserInfo.Find(UserId);
                    if (uiMo != null)
                    {
                        result = uiMo.UserSign;
                        Core.CacheTo.Set(usk, result, 5 * 60, false);
                    }
                }
                return result;
            }
        }
        #endregion
    }
}
