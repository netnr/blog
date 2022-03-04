#if Full || Ready

namespace Netnr.SharedReady
{
    public class ReadyTo
    {
        /// <summary>
        /// 编码注册
        /// </summary>
        public static void EncodingReg()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        /// <summary>
        /// 启用旧时间戳行为
        /// </summary>
        public static void LegacyTimestamp()
        {
            //https://www.npgsql.org/efcore/release-notes/6.0.html
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }
    }
}

#endif