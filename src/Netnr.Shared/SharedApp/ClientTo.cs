#if Full || App

using Microsoft.AspNetCore.Http;

namespace Netnr.SharedApp
{
    /// <summary>
    /// 客户端信息
    /// </summary>
    public class ClientTo
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="content"></param>
        public ClientTo(HttpContext content)
        {
            var header = content.Request.HttpContext.Request.Headers;

            IPv4 = content.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            //取代理IP
            if (header.ContainsKey("X-Forwarded-For"))
            {
                IPv4 = header["X-Forwarded-For"].ToString();
            }

            Language = header["Accept-Language"].ToString().Split(';')[0];
            Referer = header["Referer"].ToString();
            UserAgent = header["User-Agent"].ToString();
        }

        /// <summary>
        /// IPv4
        /// </summary>
        public string IPv4 { get; set; }

        /// <summary>
        /// IPv6
        /// </summary>
        public string IPv6 { get; set; }

        /// <summary>
        /// UA
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// 语言
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// 引荐
        /// </summary>
        public string Referer { get; set; }
    }
}

#endif