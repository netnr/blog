using System;

namespace Netnr.Domain
{
    public partial class Draw
    {
        public string DrId { get; set; }
        public int? Uid { get; set; }
        public string DrType { get; set; }
        public string DrName { get; set; }
        public string DrContent { get; set; }
        public string DrRemark { get; set; }
        public string DrCategory { get; set; }
        public int? DrOrder { get; set; }
        public DateTime? DrCreateTime { get; set; }
        public int? DrStatus { get; set; }
        public int? DrOpen { get; set; }
        public string Spare1 { get; set; }
        public string Spare2 { get; set; }
        public string Spare3 { get; set; }
    }
}
