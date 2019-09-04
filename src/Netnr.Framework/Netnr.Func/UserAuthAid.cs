using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Netnr.Func
{
    /// <summary>
    /// 用户授权
    /// </summary>
    public class UserAuthAid
    {
        private readonly HttpContext context;

        public UserAuthAid(HttpContext httpContext)
        {
            context = httpContext;
        }

        /// <summary>
        /// 写入授权
        /// </summary>
        /// <param name="user"></param>
        /// <param name="isremember"></param>
        /// <returns></returns>
        public void Set(Domain.UserInfo user, bool isremember = true)
        {
            //登录信息
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.Sid, user.UserId.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
            identity.AddClaim(new Claim(ClaimTypes.GivenName, user.Nickname ?? ""));
            identity.AddClaim(new Claim(ClaimTypes.Role, "1"));
            identity.AddClaim(new Claim(ClaimTypes.SerialNumber, user.UserSign));
            identity.AddClaim(new Claim(ClaimTypes.UserData, user.UserPhoto ?? ""));

            //配置
            var authParam = new AuthenticationProperties();
            if (isremember)
            {
                authParam.IsPersistent = true;
                authParam.ExpiresUtc = DateTime.Now.AddDays(10);
            }

            //写入
            context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), authParam);
        }

        /// <summary>
        /// 获取授权用户
        /// </summary>
        /// <returns></returns>
        public Domain.UserInfo Get()
        {
            var usermo = new Domain.UserInfo
            {
                UserId = Convert.ToInt32(context.User.FindFirst(ClaimTypes.Sid)?.Value),
                UserName = context.User.FindFirst(ClaimTypes.Name)?.Value,
                Nickname = context.User.FindFirst(ClaimTypes.GivenName)?.Value,
                UserPhoto = context.User.FindFirst(ClaimTypes.UserData)?.Value
            };

            return usermo;
        }
    }
}
