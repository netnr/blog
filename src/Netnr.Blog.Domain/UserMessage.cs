namespace Netnr.Blog.Domain
{
    /// <summary>
    /// 用户消息
    /// </summary>
    public partial class UserMessage
    {
        public string UmId { get; set; }
        /// <summary>
        /// 接收用户
        /// </summary>
        public int? Uid { get; set; }
        /// <summary>
        /// 触发用户ID
        /// </summary>
        public int? UmTriggerUid { get; set; }
        /// <summary>
        /// 消息分类
        /// </summary>
        public string UmType { get; set; }
        /// <summary>
        /// 消息目标ID
        /// </summary>
        public string UmTargetId { get; set; }
        /// <summary>
        /// 消息定向索引
        /// </summary>
        public int? UmTargetIndex { get; set; }
        /// <summary>
        /// 消息标记，1系统，2回复，3私信，4点赞，5收藏，6关注
        /// </summary>
        public int? UmAction { get; set; }
        /// <summary>
        /// 消息内容
        /// </summary>
        public string UmContent { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? UmCreateTime { get; set; }
        /// <summary>
        /// 状态，1未读，2已读
        /// </summary>
        public int? UmStatus { get; set; }
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
