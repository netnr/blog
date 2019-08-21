using System;

namespace Netnr.Domain
{
    public partial class UserWriting
    {
        public int UwId { get; set; }
        public int? Uid { get; set; }
        public int? UwCategory { get; set; }
        public string UwTitle { get; set; }
        public string UwContent { get; set; }
        public string UwContentMd { get; set; }
        public DateTime? UwCreateTime { get; set; }
        public DateTime? UwUpdateTime { get; set; }
        public int? UwLastUid { get; set; }
        public DateTime? UwLastDate { get; set; }
        public int? UwReplyNum { get; set; }
        public int? UwReadNum { get; set; }
        public int? UwOpen { get; set; }
        public int? UwLaud { get; set; }
        public int? UwMark { get; set; }
        public int? UwStatus { get; set; }
        public string Spare1 { get; set; }
        public string Spare2 { get; set; }
        public string Spare3 { get; set; }
    }
}
