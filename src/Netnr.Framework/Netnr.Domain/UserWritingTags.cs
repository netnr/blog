namespace Netnr.Domain
{
    public partial class UserWritingTags
    {
        public int UwtId { get; set; }
        public int UwId { get; set; }
        public int? TagId { get; set; }
        public string TagName { get; set; }
        public string TagCode { get; set; }
        public string Spare1 { get; set; }
        public string Spare2 { get; set; }
        public string Spare3 { get; set; }
    }
}
