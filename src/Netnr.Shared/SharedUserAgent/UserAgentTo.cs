#if Full || UserAgent

using DeviceDetectorNET;
using DeviceDetectorNET.Parser;

namespace Netnr.SharedUserAgent
{
    /// <summary>
    /// 客户端 UA 解析
    /// </summary>
    public class UserAgentTo
    {
        /// <summary>
        /// 浏览器名称
        /// </summary>
        public string BrowserName { get; set; }

        /// <summary>
        /// 浏览器类型
        /// </summary>
        public string BrowserType { get; set; }

        /// <summary>
        /// 浏览器版本
        /// </summary>
        public string BrowserVersion { get; set; }

        /// <summary>
        /// 操作系统名称
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// 操作系统简称
        /// </summary>
        public string SystemShortName { get; set; }

        /// <summary>
        /// 操作系统
        /// </summary>
        public string SystemVersion { get; set; }

        /// <summary>
        /// 操作系统平台
        /// </summary>
        public string SystemPlatform { get; set; }

        /// <summary>
        /// 是爬虫
        /// </summary>
        public bool IsBot { get; set; }

        /// <summary>
        /// 解析 User-Agent
        /// </summary>
        /// <param name="ua">User-Agent</param>
        public UserAgentTo(string ua)
        {
            DeviceDetector.SetVersionTruncation(VersionTruncation.VERSION_TRUNCATION_NONE);

            if (ua != null)
            {
                var dd = new DeviceDetector(ua);
                dd.DiscardBotInformation();

                dd.Parse();
                if (IsBot = dd.IsBot())
                {
                    var botInfo = dd.GetBot();
                    if (botInfo.Success)
                    {
                        BrowserName = botInfo.Match.Name;
                    }
                }
                else
                {
                    var clientInfo = dd.GetClient();
                    if (clientInfo.Success)
                    {
                        BrowserName = clientInfo.Match.Name;
                        BrowserType = clientInfo.Match.Type;
                        BrowserVersion = clientInfo.Match.Version;
                    }
                    var osInfo = dd.GetOs();
                    if (osInfo.Success)
                    {
                        SystemName = osInfo.Match.Name;
                        SystemShortName = osInfo.Match.ShortName;
                        SystemVersion = osInfo.Match.Version;
                        SystemPlatform = osInfo.Match.Platform;
                    }
                }
            }
        }
    }
}

#endif