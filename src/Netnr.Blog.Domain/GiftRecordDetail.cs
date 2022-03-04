namespace Netnr.Blog.Domain
{
    /// <summary>
    /// 礼薄明细
    /// </summary>
    public partial class GiftRecordDetail
    {
        public string GrdId { get; set; }
        /// <summary>
        /// 主表ID
        /// </summary>
        public string GrId { get; set; }
        /// <summary>
        /// 送礼人
        /// </summary>
        public string GrdGiverName { get; set; }
        /// <summary>
        /// 礼金
        /// </summary>
        public decimal? GrdCash { get; set; }
        /// <summary>
        /// 礼物
        /// </summary>
        public string GrdGoods { get; set; }
        /// <summary>
        /// 时间
        /// </summary>
        public DateTime? GrdCreateTime { get; set; }
        public string GrdRemark { get; set; }
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
