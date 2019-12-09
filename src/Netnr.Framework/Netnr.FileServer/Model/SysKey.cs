using SQLite;
using System;

namespace Netnr.FileServer.Model
{
    /// <summary>
    /// token管理
    /// </summary>
    public class SysKey
    {
        /// <summary>
        /// AppId   取Guid转成long，长度19位
        /// </summary>
        [PrimaryKey]
        public string SkAppId { get; set; }

        /// <summary>
        /// SkAppKey    密钥，取Guid的MD5值
        /// </summary>
        [Unique]
        public string SkAppKey { get; set; }

        /// <summary>
        /// 名称、用户
        /// </summary>
        public string SkName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? SkCreateTime { get; set; }

        /// <summary>
        /// 生成的Token
        /// </summary>
        [Unique]
        public string SkToken { get; set; }

        /// <summary>
        /// Token过期时间
        /// </summary>
        public DateTime? SkTokenExpireTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string SkRemark { get; set; }
    }
}