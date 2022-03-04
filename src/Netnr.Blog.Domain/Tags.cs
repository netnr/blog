namespace Netnr.Blog.Domain
{
    /// <summary>
    /// 标签
    /// </summary>
    public partial class Tags
    {
        public int TagId { get; set; }
        /// <summary>
        /// 标签名
        /// </summary>
        public string TagName { get; set; }
        /// <summary>
        /// 标签码
        /// </summary>
        public string TagCode { get; set; }
        /// <summary>
        /// 标签图标
        /// </summary>
        public string TagIcon { get; set; }
        /// <summary>
        /// Pid
        /// </summary>
        public int? TagPid { get; set; }
        /// <summary>
        /// 创建用户UID，系统标签为0
        /// </summary>
        public int? TagOwner { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int? TagOrder { get; set; }
        /// <summary>
        /// 状态 1启用
        /// </summary>
        public int? TagStatus { get; set; }
        /// <summary>
        /// 热度
        /// </summary>
        public int? TagHot { get; set; }
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
