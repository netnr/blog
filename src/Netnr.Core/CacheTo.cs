using System;
using System.Runtime.Caching;

namespace Netnr.Core
{
    /// <summary>
    /// 缓存
    /// </summary>
    public class CacheTo
    {
        /// <summary>
        /// 缓存
        /// </summary>
        public static MemoryCache memoryCache = MemoryCache.Default;

        /// <summary>
        /// 获取数据缓存
        /// </summary>
        /// <param name="key">键</param>
        public static object Get(string key) => memoryCache.Get(key);

        /// <summary>
        /// 设置数据缓存
        /// 变化时间过期（平滑过期）。表示缓存连续2个小时没有访问就过期（TimeSpan.FromSeconds(7200)）。
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="second">过期时间，默认7200秒 </param>
        /// <param name="sliding">是否相对过期，默认是；否，则固定时间过期</param>
        public static void Set(string key, object value, int second = 7200, bool sliding = true)
        {
            var cip = new CacheItemPolicy();

            if (sliding)
            {
                cip.SlidingExpiration = TimeSpan.FromSeconds(second);
            }
            else
            {
                cip.AbsoluteExpiration = DateTime.Now.AddSeconds(second);
            }

            memoryCache.Set(key, value, cip);
        }

        /// <summary>
        /// 设置数据缓存
        /// 变化时间过期（平滑过期）。表示缓存连续2个小时没有访问就过期（TimeSpan.FromSeconds(7200)）。
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="second">过期时间，默认7200秒 </param>
        /// <param name="sliding">是否相对过期，默认是；否，则固定时间过期</param>
        /// <param name="action">更多策略配置</param>
        public static void SetOption(string key, object value, int second = 7200, bool sliding = true, Action<CacheItemPolicy> action = null)
        {
            var cip = new CacheItemPolicy();

            if (sliding)
            {
                cip.SlidingExpiration = TimeSpan.FromSeconds(second);
            }
            else
            {
                cip.AbsoluteExpiration = DateTime.Now.AddSeconds(second);
            }

            action?.Invoke(cip);

            memoryCache.Set(key, value, cip);
        }

        /// <summary>
        /// 移除指定数据缓存
        /// </summary>
        /// <param name="key">键</param>
        public static void Remove(string key) => memoryCache.Remove(key);

        /// <summary>
        /// 移除全部缓存
        /// </summary>
        public static void RemoveAll() => memoryCache.Dispose();
    }
}