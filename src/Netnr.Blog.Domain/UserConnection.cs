namespace Netnr.Blog.Domain
{
    /// <summary>
    /// 用户关联
    /// </summary>
    public partial class UserConnection
    {
        public string UconnId { get; set; }
        public int? Uid { get; set; }
        /// <summary>
        /// 关联分类
        /// </summary>
        public string UconnTargetType { get; set; }
        /// <summary>
        /// 关联目标ID
        /// </summary>
        public string UconnTargetId { get; set; }
        /// <summary>
        /// 1点赞，2收藏，3关注
        /// </summary>
        public int? UconnAction { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? UconnCreateTime { get; set; }
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
