#if Full || DataKit

namespace Netnr.SharedDataKit
{
    /// <summary>
    /// 数据库信息
    /// </summary>
    public partial class DatabaseVM
    {
        /// <summary>
        /// 库名
        /// </summary>
        public string DatabaseName { get; set; }
        /// <summary>
        /// 所属者
        /// </summary>
        public string DatabaseOwner { get; set; }
        /// <summary>
        /// 表空间
        /// </summary>
        public string DatabaseSpace { get; set; }
        /// <summary>
        /// 字符集
        /// </summary>
        public string DatabaseCharset { get; set; }
        /// <summary>
        /// 排序规则
        /// </summary>
        public string DatabaseCollation { get; set; }
        /// <summary>
        /// 数据路径
        /// </summary>
        public string DatabasePath { get; set; }
        /// <summary>
        /// 日志路径
        /// </summary>
        public string DatabaseLogPath { get; set; }
        /// <summary>
        /// 数据大小
        /// </summary>
        public long DatabaseDataLength { get; set; }
        /// <summary>
        /// 日志大小
        /// </summary>
        public long DatabaseLogLength { get; set; }
        /// <summary>
        /// 索引大小
        /// </summary>
        public long DatabaseIndexLength { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? DatabaseCreateTime { get; set; }
    }
}

#endif