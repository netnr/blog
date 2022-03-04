#if Full || Public

namespace Netnr
{
    /// <summary>
    /// 计时
    /// </summary>
    public class SharedTimingVM
    {
        public Stopwatch sw;
        private double et = 0;

        /// <summary>
        /// 构造
        /// </summary>
        public SharedTimingVM()
        {
            sw = new Stopwatch();
            sw.Start();
        }

        /// <summary>
        /// 片段耗时，毫秒
        /// </summary>
        /// <returns></returns>
        public double PartTime()
        {
            var pt = sw.Elapsed.TotalMilliseconds - et;
            et = sw.Elapsed.TotalMilliseconds;
            return pt;
        }

        /// <summary>
        /// 片段耗时，毫秒，可视化
        /// </summary>
        /// <param name="format">格式化</param>
        /// <returns></returns>
        public string PartTimeFormat(string format = @"hh\:mm\:ss\:fff")
        {
            var pt = sw.Elapsed.TotalMilliseconds - et;
            et = sw.Elapsed.TotalMilliseconds;
            return TimeSpan.FromMilliseconds(pt).ToString(format);
        }

        /// <summary>
        /// 总耗时，毫秒
        /// </summary>
        /// <returns></returns>
        public double TotalTime()
        {
            return sw.Elapsed.TotalMilliseconds;
        }

        /// <summary>
        /// 总耗时，毫秒，可视化
        /// </summary>
        /// <param name="format">格式化</param>
        /// <returns></returns>
        public string TotalTimeFormat(string format = @"hh\:mm\:ss\:fff")
        {
            return TimeSpan.FromMilliseconds(sw.Elapsed.TotalMilliseconds).ToString(format);
        }
    }
}

#endif