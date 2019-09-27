using System;

namespace Netnr.Domain
{
    public partial class Notepad
    {
        public int NoteId { get; set; }
        public int? Uid { get; set; }
        public string NoteTitle { get; set; }
        public string NoteContent { get; set; }
        public DateTime? NoteCreateTime { get; set; }
        public DateTime? NoteUpdateTime { get; set; }
        public string Spare1 { get; set; }
        public string Spare2 { get; set; }
        public string Spare3 { get; set; }
    }
}
