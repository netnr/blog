using System;

namespace Netnr.Core
{
    /// <summary>
    /// 生成唯一标识
    /// </summary>
    public class UniqueTo
    {
        /// <summary>
        /// 根据Guid获取唯一数字序列，19位
        /// </summary>
        /// <returns></returns>
        public static long LongId()
        {
            byte[] bytes = Guid.NewGuid().ToByteArray();
            return BitConverter.ToInt64(bytes, 0);
        }
    }
}