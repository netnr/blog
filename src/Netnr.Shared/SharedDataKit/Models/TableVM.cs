#if Full || DataKit

namespace Netnr.SharedDataKit
{
    /// <summary>
    /// 表信息
    /// </summary>
    public partial class TableVM
    {
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// 模式名
        /// </summary>
        public string SchemaName { get; set; }
        /// <summary>
        /// 所属
        /// </summary>
        public string TableOwner { get; set; }
        /// <summary>
        /// 空间
        /// </summary>
        public string TableSpace { get; set; }
        /// <summary>
        /// 表类型（表、视图）
        /// </summary>
        public string TableType { get; set; }
        /// <summary>
        /// 引擎
        /// </summary>
        public string TableEngine { get; set; }
        /// <summary>
        /// 总行数
        /// </summary>
        public long TableRows { get; set; }
        /// <summary>
        /// 数据大小
        /// </summary>
        public long TableDataLength { get; set; }
        /// <summary>
        /// 索引大小
        /// </summary>
        public long TableIndexLength { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? TableCreateTime { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? TableModifyTime { get; set; }
        /// <summary>
        /// 排序规则
        /// </summary>
        public string TableCollation { get; set; }
        /// <summary>
        /// 表注释
        /// </summary>
        public string TableComment { get; set; }
    }
}

#endif