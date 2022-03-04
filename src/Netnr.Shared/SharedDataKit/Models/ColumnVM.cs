#if Full || DataKit

namespace Netnr.SharedDataKit
{
    /// <summary>
    /// 表列信息
    /// </summary>
    public partial class ColumnVM
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
        /// 表注释
        /// </summary>
        public string TableComment { get; set; }
        /// <summary>
        /// 列名
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// 数据类型及长度
        /// </summary>
        public string ColumnType { get; set; }
        /// <summary>
        /// 数据类型
        /// </summary>
        public string DataType { get; set; }
        /// <summary>
        /// 数据长度
        /// </summary>
        public string DataLength { get; set; }
        /// <summary>
        /// 数据精度
        /// </summary>
        public string DataScale { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int ColumnOrder { get; set; }
        /// <summary>
        /// 主键（大于等于1）
        /// </summary>
        public int PrimaryKey { get; set; }
        /// <summary>
        /// 自增（1：是）
        /// </summary>
        public int AutoIncr { get; set; }
        /// <summary>
        /// 可为空（1：空；0：非空）
        /// </summary>
        public int IsNullable { get; set; }
        /// <summary>
        /// 默认值
        /// </summary>
        public string ColumnDefault { get; set; }
        /// <summary>
        /// 列注释
        /// </summary>
        public string ColumnComment { get; set; }
    }
}

#endif