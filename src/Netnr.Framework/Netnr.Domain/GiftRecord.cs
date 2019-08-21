using System;

namespace Netnr.Domain
{
    public partial class GiftRecord
    {
        public string GrId { get; set; }
        public string Uid { get; set; }
        public string GrTheme { get; set; }
        public int? GrType { get; set; }
        public string GrName1 { get; set; }
        public string GrName2 { get; set; }
        public string GrName3 { get; set; }
        public string GrName4 { get; set; }
        public DateTime? GrActionTime { get; set; }
        public string GrDescription { get; set; }
        public DateTime? GrCreateTime { get; set; }
        public string GrRemark { get; set; }
        public string Spare1 { get; set; }
        public string Spare2 { get; set; }
        public string Spare3 { get; set; }
    }
}
