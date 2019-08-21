using System;

namespace Netnr.Domain
{
    public partial class UserConnection
    {
        public string UconnId { get; set; }
        public int? Uid { get; set; }
        public string UconnTargetType { get; set; }
        public string UconnTargetId { get; set; }
        public int? UconnAction { get; set; }
        public DateTime? UconnCreateTime { get; set; }
        public string Spare1 { get; set; }
        public string Spare2 { get; set; }
        public string Spare3 { get; set; }
    }
}
