using System;

namespace Netnr.Domain
{
    public partial class GiftRecordDetail
    {
        public string GrdId { get; set; }
        public string GrId { get; set; }
        public string GrdGiverName { get; set; }
        public decimal? GrdCash { get; set; }
        public string GrdGoods { get; set; }
        public DateTime? GrdCreateTime { get; set; }
        public string GrdRemark { get; set; }
        public string Spare1 { get; set; }
        public string Spare2 { get; set; }
        public string Spare3 { get; set; }
    }
}
