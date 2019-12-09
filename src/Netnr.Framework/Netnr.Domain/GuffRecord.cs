using System;

namespace Netnr.Domain
{
    public partial class GuffRecord
    {
        public string GrId { get; set; }
        public int? Uid { get; set; }
        public string GrTypeName { get; set; }
        public string GrTypeValue { get; set; }
        public string GrObject { get; set; }
        public string GrContent { get; set; }
        public string GrContentMd { get; set; }
        public string GrImage { get; set; }
        public string GrAudio { get; set; }
        public string GrVideo { get; set; }
        public string GrFile { get; set; }
        public string GrRemark { get; set; }
        public string GrTag { get; set; }
        public DateTime? GrCreateTime { get; set; }
        public DateTime? GrUpdateTime { get; set; }
        public int? GrReplyNum { get; set; }
        public int? GrOpen { get; set; }
        public int? GrReadNum { get; set; }
        public int? GrLaud { get; set; }
        public int? GrMark { get; set; }
        public int? GrStatus { get; set; }
        public string Spare1 { get; set; }
        public string Spare2 { get; set; }
        public string Spare3 { get; set; }
    }
}
