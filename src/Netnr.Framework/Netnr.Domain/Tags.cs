namespace Netnr.Domain
{
    public partial class Tags
    {
        public int TagId { get; set; }
        public string TagName { get; set; }
        public string TagCode { get; set; }
        public string TagIcon { get; set; }
        public int? TagPid { get; set; }
        public int? TagOwner { get; set; }
        public int? TagOrder { get; set; }
        public int? TagStatus { get; set; }
        public int? TagHot { get; set; }
        public string Spare1 { get; set; }
        public string Spare2 { get; set; }
        public string Spare3 { get; set; }
    }
}
