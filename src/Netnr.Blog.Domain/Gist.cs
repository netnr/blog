namespace Netnr.Blog.Domain
{
    /// <summary>
    /// 代码片段
    /// </summary>
    public partial class Gist
    {
        public string GistId { get; set; }
        /// <summary>
        /// 所属用户
        /// </summary>
        public int? Uid { get; set; }
        /// <summary>
        /// 唯一编码
        /// </summary>
        public string GistCode { get; set; }
        /// <summary>
        /// 文件名
        /// </summary>
        public string GistFilename { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string GistContent { get; set; }
        /// <summary>
        /// 预览内容，前10行
        /// </summary>
        public string GistContentPreview { get; set; }
        /// <summary>
        /// 行数
        /// </summary>
        public int? GistRow { get; set; }
        /// <summary>
        /// 语言
        /// </summary>
        public string GistLanguage { get; set; }
        /// <summary>
        /// 主题
        /// </summary>
        public string GistTheme { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string GistRemark { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public string GistTags { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? GistCreateTime { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? GistUpdateTime { get; set; }
        /// <summary>
        /// 1公开，2私有
        /// </summary>
        public int? GistOpen { get; set; }
        /// <summary>
        /// 状态 1正常
        /// </summary>
        public int? GistStatus { get; set; }
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
