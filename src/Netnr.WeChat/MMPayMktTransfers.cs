using System.Collections.Generic;
using System.Text;
using Netnr.WeChat.Helpers;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.IO;

namespace Netnr.WeChat
{
    /// <summary>
    /// 
    /// </summary>
    public class MMPayMktTransfers
    {
        /// <summary>
        /// 发放普通红包 https://pay.weixin.qq.com/wiki/doc/api/tools/cash_coupon.php?chapter=13_1
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="mch_id"></param>
        /// <param name="nonce_str"></param>
        /// <param name="partner_trade_no"></param>
        /// <param name="openid"></param>
        /// <param name="send_name"></param>
        /// <param name="amount"></param>
        /// <param name="total_num"></param>
        /// <param name="wishing"></param>
        /// <param name="client_ip"></param>
        /// <param name="act_name"></param>
        /// <param name="remark"></param>
        /// <param name="partnerKey"></param>
        /// <param name="cert"></param>
        /// <param name="certPassword"></param>
        /// <param name="scene_id"></param>
        /// <param name="risk_info"></param>
        /// <param name="consume_mch_id"></param>
        /// <returns></returns>
        public static string Sendredpack(string appid, string mch_id, string nonce_str, string partner_trade_no,
            string openid, string send_name, int amount, int total_num, string wishing, string client_ip, string act_name,
            string remark, string partnerKey, string cert, string certPassword,
            string scene_id = "", string risk_info = "", string consume_mch_id = "")
        {
            var stringADict = new Dictionary<string, string>
            {
                { "nonce_str", nonce_str },
                { "mch_billno", partner_trade_no },
                { "mch_id", mch_id },
                { "wxappid", appid },
                { "send_name", send_name },
                { "re_openid", openid },
                { "total_amount", amount.ToString() },
                { "total_num", total_num.ToString() },
                { "wishing", wishing },
                { "client_ip", client_ip },
                { "act_name", act_name },
                { "remark", remark }
            };
            if (!string.IsNullOrEmpty(scene_id))
            {
                stringADict.Add("scene_id", scene_id);
            }
            if (!string.IsNullOrEmpty(risk_info))
            {
                stringADict.Add("risk_info", risk_info);
            }
            if (!string.IsNullOrEmpty(consume_mch_id))
            {
                stringADict.Add("consume_mch_id", consume_mch_id);
            }

            var sign = PayUtil.Sign(stringADict, partnerKey);//生成签名字符串
            var postdata = PayUtil.GeneralPostdata(stringADict, sign);
            var url = "https://api.mch.weixin.qq.com/mmpaymkttransfers/sendredpack";
            X509Certificate2 cer = new(cert, certPassword);
            Encoding encoding = Encoding.UTF8;
            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
            webrequest.ClientCertificates.Add(cer);
            byte[] bs = encoding.GetBytes(postdata);
            webrequest.Method = "POST";
            webrequest.ContentType = "application/x-www-form-urlencoded";
            webrequest.ContentLength = bs.Length;
            using (Stream reqStream = webrequest.GetRequestStream())
            {
                reqStream.Write(bs, 0, bs.Length);
                reqStream.Close();
            }
            using (HttpWebResponse response = (HttpWebResponse)webrequest.GetResponse())
            {
                using (StreamReader reader = new(response.GetResponseStream(), encoding))
                {
                    var resXml = reader.ReadToEnd().ToString();
                    return resXml;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class Promotion
        {
            /// <summary>
            /// 企业付款业务
            /// 企业付款业务是基于微信支付商户平台的资金管理能力，为了协助商户方便地实现企业向个人付款，针对部分有开发能力的商户，提供通过API完成企业付款的功能。 
            /// 比如目前的保险行业向客户退保、给付、理赔。https://pay.weixin.qq.com/wiki/doc/api/tools/mch_pay.php?chapter=14_2
            /// </summary>
            /// <param name="appid"></param>
            /// <param name="mch_id"></param>
            /// <param name="nonce_str"></param>
            /// <param name="partner_trade_no"></param>
            /// <param name="openid"></param>
            /// <param name="check_name"></param>
            /// <param name="amount"></param>
            /// <param name="desc"></param>
            /// <param name="spbill_create_ip"></param>
            /// <param name="partnerKey"></param>
            /// <param name="cert"></param>
            /// <param name="certPassword"></param>
            /// <returns></returns>
            public static string Transfers(string appid, string mch_id, string nonce_str, string partner_trade_no,
                string openid, string check_name, int amount, string desc, string spbill_create_ip, string partnerKey, string cert, string certPassword)
            {
                var stringADict = new Dictionary<string, string>
                {
                    { "mch_appid", appid },
                    { "mchid", mch_id },
                    { "nonce_str", nonce_str },
                    { "partner_trade_no", partner_trade_no },
                    { "openid", openid },
                    { "check_name", check_name },
                    { "amount", amount.ToString() },
                    { "desc", desc },
                    { "spbill_create_ip", spbill_create_ip }
                };
                var sign = PayUtil.Sign(stringADict, partnerKey);//生成签名字符串
                var postdata = PayUtil.GeneralPostdata(stringADict, sign);
                var url = "https://api.mch.weixin.qq.com/mmpaymkttransfers/promotion/transfers";
                X509Certificate2 cer = new(cert, certPassword);
                Encoding encoding = Encoding.UTF8;
                HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
                webrequest.ClientCertificates.Add(cer);
                byte[] bs = encoding.GetBytes(postdata);
                webrequest.Method = "POST";
                webrequest.ContentType = "application/x-www-form-urlencoded";
                webrequest.ContentLength = bs.Length;
                using (Stream reqStream = webrequest.GetRequestStream())
                {
                    reqStream.Write(bs, 0, bs.Length);
                    reqStream.Close();
                }
                using (HttpWebResponse response = (HttpWebResponse)webrequest.GetResponse())
                {
                    using (StreamReader reader = new(response.GetResponseStream(), encoding))
                    {
                        var resXml = reader.ReadToEnd().ToString();
                        return resXml;
                    }
                }
            }
        }

    }
}
