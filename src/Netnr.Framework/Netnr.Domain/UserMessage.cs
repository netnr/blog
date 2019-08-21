using System;

namespace Netnr.Domain
{
    public partial class UserMessage
    {
        public string UmId { get; set; }
        public int? Uid { get; set; }
        public int? UmTriggerUid { get; set; }
        public string UmType { get; set; }
        public string UmTargetId { get; set; }
        public int? UmTargetIndex { get; set; }
        public int? UmAction { get; set; }
        public string UmContent { get; set; }
        public DateTime? UmCreateTime { get; set; }
        public int? UmStatus { get; set; }
        public string Spare1 { get; set; }
        public string Spare2 { get; set; }
        public string Spare3 { get; set; }
    }
}
