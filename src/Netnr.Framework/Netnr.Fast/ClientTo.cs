using System.Net;
using Microsoft.AspNetCore.Http;

namespace Netnr.Fast
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

            string ua = header["User-Agent"].ToString();
            UserAgentGet(ua);
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
        /// 浏览器名称
        /// </summary>
        public string BrowserName { get; set; }

        /// <summary>
        /// 浏览器版本
        /// </summary>
        public string BrowserVersion { get; set; }

        /// <summary>
        /// 系统名称
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// 系统类型
        /// </summary>
        public string SystemType { get; set; }

        /// <summary>
        /// 语言
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// 引荐
        /// </summary>
        public string Referer { get; set; }

        /// <summary>
        /// 提取UserAgent信息
        /// </summary>
        /// <param name="ua"></param>
        private void UserAgentGet(string ua)
        {
            string sn = "Unknown";
            if (ua.Contains("NT 5.0"))
                sn = "Windows 2000";
            else if (ua.Contains("NT 5.1"))
                ua = "Windows XP";
            else if (ua.Contains("NT 5.2"))
                sn = "Windows 2003";
            else if (ua.Contains("NT 6.0"))
                sn = "Windows Vista";
            else if (ua.Contains("NT 6.1"))
                sn = "Windows 7";
            else if (ua.Contains("NT 6.2"))
                sn = "Windows 8";
            else if (ua.Contains("NT 6.3"))
                sn = "Windows 8.1";
            else if (ua.Contains("NT 6.4") || ua.Contains("NT 10.0"))
                sn = "Windows 10";
            else if (ua.Contains("Linux"))
                sn = "Linux";
            else if (ua.Contains("Mac"))
                sn = "Mac";
            else if (ua.Contains("SunOS"))
                sn = "SunOS";
            SystemName = sn;

            SystemType = ua.Contains("WOW64") ? "64位" : "32位";

            if (ua.Contains("Chrome"))
            {
                BrowserName = "Chrome";
            }
            else if (ua.Contains("Firefox"))
            {
                BrowserName = "Firefox";
            }
            else if (ua.Contains("Firefox"))
            {
                BrowserName = "Firefox";
            }
        }

        /// <summary>
        /// 获取指定服务器的信息
        /// </summary>
        /// <param name="url">服务器地址</param>
        /// <returns></returns>
        public static HttpWebResponse Response(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            return response;
        }
    }
}