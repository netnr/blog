namespace Netnr.Blog.Domain
{
    /// <summary>
    /// 文档
    /// </summary>
    public partial class DocSet
    {
        /// <summary>
        /// 唯一编码
        /// </summary>
        public string DsCode { get; set; }
        /// <summary>
        /// 所属用户
        /// </summary>
        public int? Uid { get; set; }
        /// <summary>
        /// 主题
        /// </summary>
        public string DsName { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string DsRemark { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? DsCreateTime { get; set; }
        /// <summary>
        /// 公开1
        /// </summary>
        public int? DsOpen { get; set; }
        /// <summary>
        /// 状态1正常
        /// </summary>
        public int? DsStatus { get; set; }
        /// <summary>
        /// 分享码
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
