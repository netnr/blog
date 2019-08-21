using System;

namespace Netnr.Domain
{
    public partial class UserInfo
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int? UserNameChange { get; set; }
        public string UserPwd { get; set; }
        public string OpenId1 { get; set; }
        public string OpenId2 { get; set; }
        public string OpenId3 { get; set; }
        public string OpenId4 { get; set; }
        public string OpenId5 { get; set; }
        public string OpenId6 { get; set; }
        public string OpenId7 { get; set; }
        public string OpenId8 { get; set; }
        public string OpenId9 { get; set; }
        public DateTime? UserCreateTime { get; set; }
        public DateTime? UserLoginTime { get; set; }
        public int? LoginLimit { get; set; }
        public string UserSign { get; set; }
        public string Nickname { get; set; }
        public string UserPhoto { get; set; }
        public int? UserSex { get; set; }
        public DateTime? UserBirthday { get; set; }
        public string UserPhone { get; set; }
        public string UserMail { get; set; }
        public int? UserMailValid { get; set; }
        public string UserUrl { get; set; }
        public string UserSay { get; set; }
        public string Spare1 { get; set; }
        public string Spare2 { get; set; }
        public string Spare3 { get; set; }
    }
}
