using System;

namespace Netnr.Domain
{
    public partial class GistSync
    {
        public string GistCode { get; set; }
        public string GistFilename { get; set; }
        public int? Uid { get; set; }
        public string GsGitHubId { get; set; }
        public DateTime? GsGitHubTime { get; set; }
        public string GsGiteeId { get; set; }
        public DateTime? GsGiteeTime { get; set; }
        public string Spare1 { get; set; }
        public string Spare2 { get; set; }
        public string Spare3 { get; set; }
    }
}
