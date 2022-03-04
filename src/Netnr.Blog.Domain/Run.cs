namespace Netnr.Blog.Domain
{
    /// <summary>
    /// 运行
    /// </summary>
    public partial class Run
    {
        public string RunId { get; set; }
        /// <summary>
        /// 所属用户
        /// </summary>
        public int? Uid { get; set; }
        /// <summary>
        /// 唯一编码
        /// </summary>
        public string RunCode { get; set; }
        /// <summary>
        /// 内容 html
        /// </summary>
        public string RunContent1 { get; set; }
        /// <summary>
        /// 内容 js
        /// </summary>
        public string RunContent2 { get; set; }
        /// <summary>
        /// 内容 css
        /// </summary>
        public string RunContent3 { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string RunContent4 { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string RunContent5 { get; set; }
        /// <summary>
        /// 主题
        /// </summary>
        public string RunTheme { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string RunRemark { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public string RunTags { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? RunCreateTime { get; set; }
        /// <summary>
        /// 公开1
        /// </summary>
        public int? RunOpen { get; set; }
        /// <summary>
        /// 状态 1正常
        /// </summary>
        public int? RunStatus { get; set; }
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
