using System;

namespace Netnr.Core
{
    /// <summary>
    /// 生成随机字符
    /// </summary>
    public class RandomTo
    {
        /// <summary>
        /// 随机字符 验证码
        /// </summary>
        /// <param name="strLen">长度 默认4个字符</param>
        /// <param name="source">自定义随机的字符源</param>
        /// <returns></returns>
        public static string StrCode(int strLen = 4, string source = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")
        {
            string result = string.Empty;
            if (strLen > 0)
            {
                Random rd = new(Guid.NewGuid().GetHashCode());
                for (int i = 0; i < strLen; i++)
                    result += source[rd.Next(source.Length)].ToString();
            }
            return result;
        }

        /// <summary>
        /// 随机字符 纯数字
        /// </summary>
        /// <param name="strLen">长度 默认4</param>
        /// <param name="source">生成的源字符串 默认0-9</param>
        /// <returns></returns>
        public static string NumCode(int strLen = 4, string source = "0123456789")
        {
            return StrCode(strLen, source);
        }
    }
}