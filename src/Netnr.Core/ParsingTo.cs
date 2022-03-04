using System;
using System.Text.RegularExpressions;

namespace Netnr.Core
{
    /// <summary>
    /// 解析
    /// </summary>
    public class ParsingTo
    {
        /// <summary>
        /// 移除注释
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string RemoveComment(string input)
        {
            var blockComments = @"/\*(.*?)\*/";
            var lineComments = @"//(.*?)\r?\n";
            var strings = @"""((\\[^\n]|[^""\n])*)""";
            var verbatimStrings = @"@(""[^""]*"")+";

            string noComments = Regex.Replace(input, blockComments + "|" + lineComments + "|" + strings + "|" + verbatimStrings, me =>
            {
                if (me.Value.StartsWith("/*") || me.Value.StartsWith("//"))
                    return me.Value.StartsWith("//") ? Environment.NewLine : "";
                return me.Value;
            }, RegexOptions.Singleline);

            return noComments;
        }

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
                return Regex.IsMatch(txt, @"\w[-\w.+]*@([A-Za-z0-9][-A-Za-z0-9]+\.)+[A-Za-z]{2,14}");
            }
        }

        /// <summary>
        /// 是合法链接路径（数字、字母、下划线）；可为多级路径，如：abc/xyz ；为空时返回不合法
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static bool IsLinkPath(string txt)
        {
            if (string.IsNullOrWhiteSpace(txt))
            {
                return false;
            }
            else
            {
                return !Regex.IsMatch(txt.Replace("/", ""), @"\W");
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

        /// <summary>
        /// 字节可视化
        /// </summary>
        /// <param name="size">字节大小</param>
        /// <param name="keep">保留</param>
        /// <returns></returns>
        public static string FormatByteSize(double size, int keep = 2)
        {
            string[] suffixes = new[] { " B", " KB", " MB", " GB", " TB", " PB" };
            const double unit = 1024;
            int i = 0;
            while (size > unit)
            {
                size /= unit;
                i++;
            }
            return Math.Round(size, keep) + suffixes[i];
        }

        /// <summary>
        /// 毫秒可视化
        /// </summary>
        /// <param name="ms">秒</param>
        /// <param name="format">格式化</param>
        /// <returns></returns>
        public static string FormatMillisecondsSize(double ms, string format = @"hh\:mm\:ss\:fff")
        {
            TimeSpan time = TimeSpan.FromMilliseconds(ms);
            return time.ToString(format);
        }
    }
}