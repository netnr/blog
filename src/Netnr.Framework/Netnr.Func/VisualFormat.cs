using System;

namespace Netnr.Func
{
    /// <summary>
    /// 可视 格式化
    /// </summary>
    public class VisualFormat
    {
        /// <summary>
        /// 时间格式化
        /// 如：1分钟前、1小时前
        /// </summary>
        /// <param name="dt">时间</param>
        /// <param name="reply">0发表</param>
        /// <returns></returns>
        public static string Duration(DateTime dt, int? reply = null)
        {
            TimeSpan ts = DateTime.Now - dt;
            string result = "";
            if (ts.Days > 90)
            {
                result = dt.ToString("yyyy年MM月dd日");
            }
            else if (ts.Days > 29 && ts.Days < 91)
            {
                result = Math.Floor(Convert.ToDecimal(ts.Days) / 30).ToString() + "个月前";
            }
            else if (ts.Days > 0 && ts.Days < 30)
            {
                result = ts.Days + "天前";
            }
            else if (ts.Days == 0)
            {
                if (ts.Hours != 0)
                {
                    result = ts.Hours + "小时前";
                }
                else
                {
                    if (ts.Minutes != 0)
                    {
                        result = ts.Minutes + "分钟前";
                    }
                    else
                    {
                        result = "几秒前";
                    }
                }
            }
            if (reply.HasValue)
            {
                return result + (reply == 0 ? "发表" : "回复");
            }
            return result;
        }

        /// <summary>
        /// 计数格式化，1、1.2K，3M
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string Count(int num)
        {
            var vc = num.ToString();
            if (num >= 1024 && num < 1024 * 1024)
            {
                vc = Convert.ToDecimal(num * 1.0 / 1024).ToString("f1") + "K";
            }
            else if (num >= 1024 * 1024)
            {
                vc = Convert.ToDecimal(num * 1.0 / 1024 / 1024).ToString("f1") + "M";
            }

            return vc;
        }
    }
}
