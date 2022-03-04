#if Full || Logging

using System;

namespace Netnr.SharedLogging
{
    /// <summary>
    /// 日志
    /// </summary>
    public class LoggingModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public string LogId { get; set; }

        /// <summary>
        /// 应用
        /// </summary>
        public string LogApp { get; set; }

        /// <summary>
        /// 账号
        /// </summary>
        public string LogUid { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string LogNickname { get; set; }

        /// <summary>
        /// 动作
        /// </summary>
        public string LogAction { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string LogContent { get; set; }

        /// <summary>
        /// 链接
        /// </summary>
        public string LogUrl { get; set; }

        /// <summary>
        /// 引荐
        /// </summary>
        public string LogReferer { get; set; }

        /// <summary>
        /// IP
        /// </summary>
        public string LogIp { get; set; }

        /// <summary>
        /// IP归属地
        /// </summary>
        public string LogArea { get; set; }

        /// <summary>
        /// 客户端信息
        /// </summary>
        public string LogUserAgent { get; set; }

        /// <summary>
        /// 浏览器
        /// </summary>
        public string LogBrowserName { get; set; }

        /// <summary>
        /// 操作系统
        /// </summary>
        public string LogSystemName { get; set; }

        /// <summary>
        /// 分组（1：默认；2：爬虫）
        /// </summary>
        public string LogGroup { get; set; }

        /// <summary>
        /// 级别（F： Fatal；E：Error；W：Warn；I：Info；D：Debug；A：All）
        /// </summary>
        public string LogLevel { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime LogCreateTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string LogRemark { get; set; }

        /// <summary>
        /// 备用
        /// </summary>
        public string LogSpare1 { get; set; }

        /// <summary>
        /// 备用
        /// </summary>
        public string LogSpare2 { get; set; }

        /// <summary>
        /// 备用
        /// </summary>
        public string LogSpare3 { get; set; }
    }
}

#endif