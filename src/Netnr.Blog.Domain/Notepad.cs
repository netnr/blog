namespace Netnr.Blog.Domain
{
    /// <summary>
    /// 记事本
    /// </summary>
    public partial class Notepad
    {
        public int NoteId { get; set; }
        /// <summary>
        /// 所属用户ID
        /// </summary>
        public int? Uid { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string NoteTitle { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string NoteContent { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? NoteCreateTime { get; set; }
        public DateTime? NoteUpdateTime { get; set; }
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
