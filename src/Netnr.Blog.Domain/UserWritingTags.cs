namespace Netnr.Blog.Domain
{
    /// <summary>
    /// 写作标签关联
    /// </summary>
    public partial class UserWritingTags
    {
        public int UwtId { get; set; }
        /// <summary>
        /// 写作表ID
        /// </summary>
        public int UwId { get; set; }
        /// <summary>
        /// 标签表ID
        /// </summary>
        public int? TagId { get; set; }
        /// <summary>
        /// 标签名
        /// </summary>
        public string TagName { get; set; }
        /// <summary>
        /// 标签编码
        /// </summary>
        public string TagCode { get; set; }
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
