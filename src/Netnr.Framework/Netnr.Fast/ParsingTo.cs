using System.Text.RegularExpressions;

namespace Netnr.Fast
{
    /// <summary>
    /// 解析
    /// </summary>
    public class ParsingTo
    {
        /// <summary>
        /// 是邮件地址
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static bool IsMail(string txt)
        {
            if (string.IsNullOrWhiteSpace(txt))
            {
                return false;
            }
            else
            {
                var reg = @"\w[-\w.+]*@([A-Za-z0-9][-A-Za-z0-9]+\.)+[A-Za-z]{2,14}";
                return Regex.IsMatch(txt, reg);
            }
        }

        /// <summary>
        /// JS安全拼接
        /// </summary>
        /// <param name="txt">内容</param>
        /// <returns></returns>
        public static string JsSafeJoin(string txt)
        {
            if (string.IsNullOrWhiteSpace(txt))
            {
                return txt;
            }
            return txt.Replace("'", "").Replace("\"", "");
        }
    }
}
