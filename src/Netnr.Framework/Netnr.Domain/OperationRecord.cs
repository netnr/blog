using System;

namespace Netnr.Domain
{
    public partial class OperationRecord
    {
        public string OrId { get; set; }
        public string OrType { get; set; }
        public string OrAction { get; set; }
        public string OrSource { get; set; }
        public DateTime? OrCreateTime { get; set; }
        public string OrMark { get; set; }
        public string OrRemark { get; set; }
        public string Spare1 { get; set; }
        public string Spare2 { get; set; }
        public string Spare3 { get; set; }
    }
}
