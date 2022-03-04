namespace Netnr.Blog.Domain
{
    /// <summary>
    /// 键值同义词
    /// </summary>
    public partial class KeyValueSynonym
    {
        public string KsId { get; set; }
        /// <summary>
        /// 键名
        /// </summary>
        public string KeyName { get; set; }
        /// <summary>
        /// 键名 同义词
        /// </summary>
        public string KsName { get; set; }
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
