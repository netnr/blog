namespace Netnr.Blog.Domain
{
    /// <summary>
    /// 用户写作
    /// </summary>
    public partial class UserWriting
    {
        public int UwId { get; set; }
        public int? Uid { get; set; }
        /// <summary>
        /// 所属分类
        /// </summary>
        public int? UwCategory { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string UwTitle { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string UwContent { get; set; }
        /// <summary>
        /// 内容Markdown
        /// </summary>
        public string UwContentMd { get; set; }
        /// <summary>
        /// 初始发布时间
        /// </summary>
        public DateTime? UwCreateTime { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UwUpdateTime { get; set; }
        /// <summary>
        /// 最后回复人
        /// </summary>
        public int? UwLastUid { get; set; }
        /// <summary>
        /// 最后回复时间
        /// </summary>
        public DateTime? UwLastDate { get; set; }
        /// <summary>
        /// 回复数量
        /// </summary>
        public int? UwReplyNum { get; set; }
        /// <summary>
        /// 阅读量
        /// </summary>
        public int? UwReadNum { get; set; }
        /// <summary>
        /// 1公开，2私有
        /// </summary>
        public int? UwOpen { get; set; }
        /// <summary>
        /// 点赞数
        /// </summary>
        public int? UwLaud { get; set; }
        /// <summary>
        /// 点赞数
        /// </summary>
        public int? UwMark { get; set; }
        /// <summary>
        /// 状态，1正常，2block，-1只读
        /// </summary>
        public int? UwStatus { get; set; }
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
