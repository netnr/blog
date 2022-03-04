using Netnr.WeChat.Helpers;
using System.Collections.Generic;

namespace Netnr.WeChat
{
    /// <summary>
    /// 
    /// </summary>
    public class Tools
    {
        /// <summary>
        /// 公共API => 转换短链接
        /// 不需要证书
        /// http://pay.weixin.qq.com/wiki/doc/api/index.php?chapter=9_9
        ///应用场景 
        ///该接口主要用于扫码原生支付模式一中的二维码链接转成短链接(weixin://wxpay/s/XXXXXX)，减小二维码数据量，提升扫描速度和精确度。
        /// </summary>
        /// <param name="appid">(必填) String(32) 微信分配的公众账号ID</param>
        /// <param name="mch_id">(必填) String(32) 微信支付分配的商户号</param>
        /// <param name="long_url">(必填) String(512) URL链接 需要转换的URL，签名用原串，传输需URL encode  </param>
        /// <param name="nonce_str">(必填) 随机字符串 随机字符串，不长于32位。</param>
        /// <param name="partnerKey">(必填)API密钥</param>
        /// <returns> 参见：http://pay.weixin.qq.com/wiki/doc/api/index.php?chapter=9_9 </returns>
        public static string ShortUrl(string appid, string mch_id, string long_url, string nonce_str,
                                     string partnerKey)
        {
            var stringADict = new Dictionary<string, string>
            {
                { "appid", appid },
                { "mch_id", mch_id },
                { "nonce_str", nonce_str },
                { "long_url", long_url }
            };

            var sign = PayUtil.Sign(stringADict, partnerKey);//生成签名字符串
            var postdata = PayUtil.GeneralPostdata(stringADict, sign);
            var url = "https://api.mch.weixin.qq.com/tools/shorturl";

            var result = NetnrCore.HttpTo.Post(url, postdata);
            return result;
        }
    }
}
