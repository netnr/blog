using System.Text.RegularExpressions;

namespace Netnr.Fast
{
    /// <summary>
    /// 正则
    /// </summary>
    public class RegexTo
    {
        /// <summary>
        /// 是邮件地址
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static bool IsMail(string txt)
        {
            var reg = @"\w[-\w.+]*@([A-Za-z0-9][-A-Za-z0-9]+\.)+[A-Za-z]{2,14}";
            return Regex.IsMatch(txt, reg);
        }
    }
}
