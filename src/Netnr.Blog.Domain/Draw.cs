namespace Netnr.Blog.Domain
{
    /// <summary>
    /// 绘制
    /// </summary>
    public partial class Draw
    {
        public string DrId { get; set; }
        /// <summary>
        /// 创建用户
        /// </summary>
        public int? Uid { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string DrType { get; set; }
        /// <summary>
        /// 分类：Draw、Mind
        /// </summary>
        public string DrName { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string DrContent { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string DrRemark { get; set; }
        /// <summary>
        /// 类别
        /// </summary>
        public string DrCategory { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int? DrOrder { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? DrCreateTime { get; set; }
        /// <summary>
        /// 状态：1正常，-1删除
        /// </summary>
        public int? DrStatus { get; set; }
        /// <summary>
        /// 公开：1公开，2私有
        /// </summary>
        public int? DrOpen { get; set; }
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
