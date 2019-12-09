using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;

namespace Netnr.FileServer.Filters
{
    /// <summary>
    /// 过滤器
    /// </summary>
    public class FilterConfigs
    {
        /// <summary>
        /// 允许跨域
        /// </summary>
        public class AllowCors : Attribute, IActionFilter
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="context"></param>
            public void OnActionExecuted(ActionExecutedContext context)
            {

            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="context"></param>
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
    }
}
