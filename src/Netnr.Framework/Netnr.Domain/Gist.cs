using System;

namespace Netnr.Domain
{
    public partial class Gist
    {
        public string GistId { get; set; }
        public int? Uid { get; set; }
        public string GistCode { get; set; }
        public string GistFilename { get; set; }
        public string GistContent { get; set; }
        public string GistContentPreview { get; set; }
        public int? GistRow { get; set; }
        public string GistLanguage { get; set; }
        public string GistTheme { get; set; }
        public string GistRemark { get; set; }
        public string GistTags { get; set; }
        public DateTime? GistCreateTime { get; set; }
        public DateTime? GistUpdateTime { get; set; }
        public int? GistOpen { get; set; }
        public int? GistStatus { get; set; }
        public string Spare1 { get; set; }
        public string Spare2 { get; set; }
        public string Spare3 { get; set; }
    }
}
