namespace Netnr.Blog.Domain
{
    /// <summary>
    /// 用户
    /// </summary>
    public partial class UserInfo
    {
        public int UserId { get; set; }
        /// <summary>
        /// 登录帐号
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 账号变更，1已经更改
        /// </summary>
        public int? UserNameChange { get; set; }
        /// <summary>
        /// 登录密码
        /// </summary>
        public string UserPwd { get; set; }
        /// <summary>
        /// 三方，QQ
        /// </summary>
        public string OpenId1 { get; set; }
        /// <summary>
        /// 三方，Weibo
        /// </summary>
        public string OpenId2 { get; set; }
        /// <summary>
        /// 三方，GitHub
        /// </summary>
        public string OpenId3 { get; set; }
        /// <summary>
        /// 三方，Taobao
        /// </summary>
        public string OpenId4 { get; set; }
        /// <summary>
        /// 三方，Microsoft
        /// </summary>
        public string OpenId5 { get; set; }
        /// <summary>
        /// 三方，DingTalk
        /// </summary>
        public string OpenId6 { get; set; }
        /// <summary>
        /// 三方
        /// </summary>
        public string OpenId7 { get; set; }
        /// <summary>
        /// 三方
        /// </summary>
        public string OpenId8 { get; set; }
        /// <summary>
        /// 三方
        /// </summary>
        public string OpenId9 { get; set; }
        /// <summary>
        /// 注册时间
        /// </summary>
        public DateTime? UserCreateTime { get; set; }
        /// <summary>
        /// 最后登录时间
        /// </summary>
        public DateTime? UserLoginTime { get; set; }
        /// <summary>
        /// 注册时间
        /// </summary>
        public DateTime? UserAddTime { get; set; }
        /// <summary>
        /// 登录限制 1限制 2补齐信息
        /// </summary>
        public int? LoginLimit { get; set; }
        /// <summary>
        /// 登录标记
        /// </summary>
        public string UserSign { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string Nickname { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        public string UserPhoto { get; set; }
        /// <summary>
        /// 性别，1男，2女
        /// </summary>
        public int? UserSex { get; set; }
        /// <summary>
        /// 生日
        /// </summary>
        public DateTime? UserBirthday { get; set; }
        /// <summary>
        /// 手机
        /// </summary>
        public string UserPhone { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        public string UserMail { get; set; }
        /// <summary>
        /// 邮箱是否验证，1验证
        /// </summary>
        public int? UserMailValid { get; set; }
        /// <summary>
        /// 网址
        /// </summary>
        public string UserUrl { get; set; }
        /// <summary>
        /// 说
        /// </summary>
        public string UserSay { get; set; }
        /// <summary>
        /// 备用
        /// </summary>
        public string Spare1 { get; set; }
        /// <summary>
        /// 备用
        /// </summary>
        public string Spare2 { get; set; }
        /// <summary>
        /// 备用
        /// </summary>
        public string Spare3 { get; set; }
    }
}
