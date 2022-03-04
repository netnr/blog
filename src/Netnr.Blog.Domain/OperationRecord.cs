namespace Netnr.Blog.Domain
{
    /// <summary>
    /// 操作记录
    /// </summary>
    public partial class OperationRecord
    {
        public string OrId { get; set; }
        /// <summary>
        /// 操作分类，推荐虚拟表名
        /// </summary>
        public string OrType { get; set; }
        /// <summary>
        /// 动作，具体的增删改等
        /// </summary>
        public string OrAction { get; set; }
        /// <summary>
        /// 源
        /// </summary>
        public string OrSource { get; set; }
        /// <summary>
        /// 时间
        /// </summary>
        public DateTime? OrCreateTime { get; set; }
        /// <summary>
        /// 标记
        /// </summary>
        public string OrMark { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string OrRemark { get; set; }
        /// <summary>
        /// 备用
        /// </summary>
        public string Spare1 { get; set; }
        /// <summary>
        /// 备用
        /// </summary>
        public string Spare2 { get; set; }
        /// <summary>
        /// 备用
        /// </summary>
        public string Spare3 { get; set; }
    }
}
