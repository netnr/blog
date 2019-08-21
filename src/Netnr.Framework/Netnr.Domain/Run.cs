using System;

namespace Netnr.Domain
{
    public partial class Run
    {
        public string RunId { get; set; }
        public int? Uid { get; set; }
        public string RunCode { get; set; }
        public string RunContent1 { get; set; }
        public string RunContent2 { get; set; }
        public string RunContent3 { get; set; }
        public string RunContent4 { get; set; }
        public string RunContent5 { get; set; }
        public string RunTheme { get; set; }
        public string RunRemark { get; set; }
        public string RunTags { get; set; }
        public DateTime? RunCreateTime { get; set; }
        public int? RunOpen { get; set; }
        public int? RunStatus { get; set; }
        public string Spare1 { get; set; }
        public string Spare2 { get; set; }
        public string Spare3 { get; set; }
    }
}
