using System;

namespace Netnr.Domain
{
    public partial class DocSetDetail
    {
        public string DsdId { get; set; }
        public string DsdPid { get; set; }
        public int? Uid { get; set; }
        public string DsCode { get; set; }
        public string DsdTitle { get; set; }
        public string DsdContentMd { get; set; }
        public string DsdContentHtml { get; set; }
        public DateTime? DsdCreateTime { get; set; }
        public DateTime? DsdUpdateTime { get; set; }
        public int? DsdOrder { get; set; }
        public string Spare1 { get; set; }
        public string Spare2 { get; set; }
        public string Spare3 { get; set; }
    }
}
