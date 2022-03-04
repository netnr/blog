using System.Security.Claims;
using Netnr.SharedFast;

namespace Netnr.Blog.Web.Apps
{
    /// <summary>
    /// 登录
    /// </summary>
    public class LoginService
    {
        /// <summary>
        /// 获取授权用户
        /// </summary>
        /// <returns></returns>
        public static Domain.UserInfo Get(HttpContext context)
        {
            var user = context.User;

            if (user.Identity.IsAuthenticated)
            {
                return new Domain.UserInfo
                {
                    UserId = Convert.ToInt32(user.FindFirst(ClaimTypes.PrimarySid)?.Value),
                    UserName = user.FindFirst(ClaimTypes.Name)?.Value,
                    Nickname = user.FindFirst(ClaimTypes.GivenName)?.Value,
                    UserSign = user.FindFirst(ClaimTypes.Sid)?.Value,
                    UserPhoto = user.FindFirst(ClaimTypes.UserData)?.Value
                };
            }
            else
            {
                var token = context.Request.Query["token"].ToString();
                var mo = TokenValid(token);
                if (mo == null)
                {
                    mo = new Domain.UserInfo();
                }
                return mo;
            }
        }

        /// <summary>
        /// 生成Token
        /// </summary>
        /// <param name="mo">授权用户信息</param>
        /// <returns></returns>
        public static string TokenMake(Domain.UserInfo mo)
        {
            var key = GlobalTo.GetValue("VerifyCode:Key");

            var token = Core.CalcTo.AESEncrypt(new
            {
                mo = new
                {
                    mo.UserId,
                    mo.UserName,
                    mo.Nickname,
                    mo.UserSign,
                    mo.UserPhoto
                },
                expired = DateTime.Now.AddDays(10).ToTimestamp()
            }.ToJson(), key);

            return token;
        }

        /// <summary>
        /// 验证Token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Domain.UserInfo TokenValid(string token)
        {
            Domain.UserInfo mo = null;

            try
            {
                if (!string.IsNullOrWhiteSpace(token))
                {
                    var key = GlobalTo.GetValue("VerifyCode:Key");

                    var jo = Core.CalcTo.AESDecrypt(token, key).ToJObject();

                    if (DateTime.Now.ToTimestamp() < long.Parse(jo["expired"].ToString()))
                    {
                        mo = jo["mo"].ToString().ToEntity<Domain.UserInfo>();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return mo;
        }

        /// <summary>
        /// 信息完整
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static SharedResultVM CompleteInfoValid(HttpContext context)
        {
            var vm = new SharedResultVM();

            if (!context.User.Identity.IsAuthenticated)
            {
                vm.Log.Add("先登录");
            }
            else
            {
                if (GlobalTo.GetValue<bool>("Common:CompleteInfo"))
                {
                    var uinfo = Get(context);

                    if (string.IsNullOrWhiteSpace(uinfo.Nickname))
                    {
                        vm.Log.Add("填写昵称");
                    }

                    using var db = Data.ContextBaseFactory.CreateDbContext();
                    var umo = db.UserInfo.Find(uinfo.UserId);

                    if (umo.UserId != GlobalTo.GetValue<int>("Common:AdminId"))
                    {
                        if (umo.UserMailValid != 1)
                        {
                            vm.Log.Add("验证邮箱");
                        }

                        if (string.IsNullOrWhiteSpace(umo.UserPhone) || umo.UserPhone.Trim().Length != 11)
                        {
                            vm.Log.Add("填写手机号码");
                        }

                        if (string.IsNullOrWhiteSpace(umo.OpenId1)
                            && string.IsNullOrWhiteSpace(umo.OpenId2)
                            && string.IsNullOrWhiteSpace(umo.OpenId3)
                            && string.IsNullOrWhiteSpace(umo.OpenId4)
                            && string.IsNullOrWhiteSpace(umo.OpenId5)
                            && string.IsNullOrWhiteSpace(umo.OpenId6))
                        {
                            vm.Log.Add("绑定一项授权关联");
                        }

                        if (umo.UserCreateTime.Value.AddDays(15) > DateTime.Now)
                        {
                            vm.Log.Add("新注册用户需 15 天以后才能操作");
                        }
                    }
                }
            }

            vm.Set(vm.Log.Count == 0);
            vm.Msg = string.Join("、", vm.Log);

            return vm;
        }

    }
}
