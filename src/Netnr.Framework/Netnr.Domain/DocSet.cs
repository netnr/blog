using System;

namespace Netnr.Domain
{
    public partial class DocSet
    {
        public string DsCode { get; set; }
        public int? Uid { get; set; }
        public string DsName { get; set; }
        public string DsRemark { get; set; }
        public DateTime? DsCreateTime { get; set; }
        public int? DsOpen { get; set; }
        public int? DsStatus { get; set; }
        public string Spare1 { get; set; }
        public string Spare2 { get; set; }
        public string Spare3 { get; set; }
    }
}
