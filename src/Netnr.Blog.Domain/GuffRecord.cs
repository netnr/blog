namespace Netnr.Blog.Domain
{
    /// <summary>
    /// 尬服列表
    /// </summary>
    public partial class GuffRecord
    {
        public string GrId { get; set; }
        /// <summary>
        /// 创建用户
        /// </summary>
        public int? Uid { get; set; }
        /// <summary>
        /// 分类，直播、名人、书、音乐等
        /// </summary>
        public string GrTypeName { get; set; }
        /// <summary>
        /// 分类值，如分类为斗鱼，值可为房间号
        /// </summary>
        public string GrTypeValue { get; set; }
        /// <summary>
        /// 对象，多个逗号分割，如主播姓名
        /// </summary>
        public string GrObject { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string GrContent { get; set; }
        /// <summary>
        /// 内容Markdown
        /// </summary>
        public string GrContentMd { get; set; }
        /// <summary>
        /// 图片，多个逗号分割
        /// </summary>
        public string GrImage { get; set; }
        /// <summary>
        /// 音频，多个逗号分割
        /// </summary>
        public string GrAudio { get; set; }
        /// <summary>
        /// 视频，多个逗号分割
        /// </summary>
        public string GrVideo { get; set; }
        /// <summary>
        /// 文件，多个逗号分割
        /// </summary>
        public string GrFile { get; set; }
        /// <summary>
        /// 结束语
        /// </summary>
        public string GrRemark { get; set; }
        /// <summary>
        /// 标签，多个逗号分割
        /// </summary>
        public string GrTag { get; set; }
        /// <summary>
        /// 初始发布时间
        /// </summary>
        public DateTime? GrCreateTime { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? GrUpdateTime { get; set; }
        /// <summary>
        /// 1公开，2私有
        /// </summary>
        public int? GrReplyNum { get; set; }
        /// <summary>
        /// 回复数
        /// </summary>
        public int? GrOpen { get; set; }
        /// <summary>
        /// 阅读量
        /// </summary>
        public int? GrReadNum { get; set; }
        /// <summary>
        /// 点赞数
        /// </summary>
        public int? GrLaud { get; set; }
        /// <summary>
        /// 收藏数
        /// </summary>
        public int? GrMark { get; set; }
        /// <summary>
        /// 状态，1正常，2block，-1只读
        /// </summary>
        public int? GrStatus { get; set; }
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
