namespace Netnr.Blog.Domain
{
    /// <summary>
    /// 用户回复
    /// </summary>
    public partial class UserReply
    {
        public int UrId { get; set; }
        /// <summary>
        /// 登录用户ID，匿名用户为0
        /// </summary>
        public int? Uid { get; set; }
        /// <summary>
        /// 匿名用户
        /// </summary>
        public string UrAnonymousName { get; set; }
        /// <summary>
        /// 匿名链接
        /// </summary>
        public string UrAnonymousLink { get; set; }
        /// <summary>
        /// 匿名邮箱
        /// </summary>
        public string UrAnonymousMail { get; set; }
        /// <summary>
        /// 目标分类
        /// </summary>
        public string UrTargetType { get; set; }
        /// <summary>
        /// 目标ID
        /// </summary>
        public string UrTargetId { get; set; }
        /// <summary>
        /// 回复内容
        /// </summary>
        public string UrContent { get; set; }
        /// <summary>
        /// 回复内容
        /// </summary>
        public string UrContentMd { get; set; }
        /// <summary>
        /// 回复时间
        /// </summary>
        public DateTime? UrCreateTime { get; set; }
        /// <summary>
        /// 状态，1正常，2block
        /// </summary>
        public int? UrStatus { get; set; }
        /// <summary>
        /// 目标PID
        /// </summary>
        public int? UrTargetPid { get; set; }
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
