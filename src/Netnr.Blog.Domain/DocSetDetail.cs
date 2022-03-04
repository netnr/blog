namespace Netnr.Blog.Domain
{
    /// <summary>
    /// 文档明细
    /// </summary>
    public partial class DocSetDetail
    {
        public string DsdId { get; set; }
        /// <summary>
        /// 父ID
        /// </summary>
        public string DsdPid { get; set; }
        /// <summary>
        /// 所属用户
        /// </summary>
        public int? Uid { get; set; }
        /// <summary>
        /// 文档集唯一编码
        /// </summary>
        public string DsCode { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string DsdTitle { get; set; }
        /// <summary>
        /// 内容Markdown
        /// </summary>
        public string DsdContentMd { get; set; }
        /// <summary>
        /// 内容Html
        /// </summary>
        public string DsdContentHtml { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string DsdContent { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? DsdCreateTime { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? DsdUpdateTime { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int? DsdOrder { get; set; }
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
        /// <summary>
        /// 1是最新
        /// </summary>
        public int? DsdLetest { get; set; }
    }
}
