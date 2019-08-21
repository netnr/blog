using System;

namespace Netnr.Domain
{
    public partial class UserReply
    {
        public int UrId { get; set; }
        public int? Uid { get; set; }
        public string UrAnonymousName { get; set; }
        public string UrAnonymousLink { get; set; }
        public string UrAnonymousMail { get; set; }
        public string UrTargetType { get; set; }
        public string UrTargetId { get; set; }
        public string UrContent { get; set; }
        public string UrContentMd { get; set; }
        public DateTime? UrCreateTime { get; set; }
        public int? UrStatus { get; set; }
        public int? UrTargetPid { get; set; }
        public string Spare1 { get; set; }
        public string Spare2 { get; set; }
        public string Spare3 { get; set; }
    }
}
