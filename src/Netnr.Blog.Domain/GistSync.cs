namespace Netnr.Blog.Domain
{
    /// <summary>
    /// 代码片段同步
    /// </summary>
    public partial class GistSync
    {
        public string GistCode { get; set; }
        /// <summary>
        /// 文件名
        /// </summary>
        public string GistFilename { get; set; }
        /// <summary>
        /// 所属用户
        /// </summary>
        public int? Uid { get; set; }
        /// <summary>
        /// GitHub的ID
        /// </summary>
        public string GsGitHubId { get; set; }
        /// <summary>
        /// GitHub最后同步时间，对应修改时间
        /// </summary>
        public DateTime? GsGitHubTime { get; set; }
        /// <summary>
        /// Gitee的ID
        /// </summary>
        public string GsGiteeId { get; set; }
        /// <summary>
        /// Gitee最后同步时间
        /// </summary>
        public DateTime? GsGiteeTime { get; set; }
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
