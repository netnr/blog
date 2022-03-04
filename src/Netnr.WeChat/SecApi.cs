using Netnr.WeChat.Helpers;
using System.Collections.Generic;

namespace Netnr.WeChat
{
    /// <summary>
    /// 
    /// </summary>
    public class SecApi
    {
        /// <summary>
        /// 
        /// </summary>
        public class Pay
        {
            /// <summary>
            /// 撤销支付API
            /// http://pay.weixin.qq.com/wiki/doc/api/index.php?chapter=5_6
            /// 应用场景:支付交易返回失败或支付系统超时，调用该接口撤销交易。如果此订单用户支付失败，微信支付系统会将此订单关闭；如果用户支付成功，微信支付系统会将此订单资金退还给用户。
            /// 注意：7天以内的交易单可调用撤销，其他正常支付的单如需实现相同功能请调用申请退款API。提交支付交易后调用【查询订单API】，没有明确的支付结果再调用【撤销订单API】。
            /// 请求需要双向证书
            /// </summary>
            /// <param name="appid">(必填) String(32) 微信分配的公众账号ID</param>
            /// <param name="mch_id">(必填) String(32) 微信支付分配的商户号</param>
            /// <param name="transaction_id"> String(32) 微信订单号 优先使用</param>
            /// <param name="out_trade_no">String(32) 商户订单号 </param>
            /// <param name="nonce_str">(必填) String(32) 随机字符串,不长于32位</param>
            /// <param name="partnerKey">API密钥</param>
            /// <returns></returns>
            public static string Reverse(string appid, string mch_id, string transaction_id,
                                      string out_trade_no, string nonce_str,
                                      string partnerKey)
            {
                var stringADict = new Dictionary<string, string>
                {
                    { "appid", appid },
                    { "mch_id", mch_id },
                    { "transaction_id", transaction_id },
                    { "out_trade_no", out_trade_no },
                    { "nonce_str", nonce_str }
                };
                var sign = PayUtil.Sign(stringADict, partnerKey);//生成签名字符串
                var postdata = PayUtil.GeneralPostdata(stringADict, sign);
                var url = "https://api.mch.weixin.qq.com/secapi/pay/reverse";

                var result = NetnrCore.HttpTo.Post(url, postdata);
                return result;
            }

            /// <summary>
            /// 公共API => 申请退款
            /// 需要双向证书
            /// http://pay.weixin.qq.com/wiki/doc/api/index.php?chapter=9_4
            ///应用场景 
            ///当交易发生之后一段时间内，由于买家或者卖家的原因需要退款时，卖家可以通过退款接口将支付款退还给买家，微信支付将在收到退款请求并且验证成功之后，按照退款规则将支付款按原路退到买家帐号上。 
            ///注意： 
            ///1.交易时间超过半年的订单无法提交退款； 
            ///2.微信支付退款支持单笔交易分多次退款，多次退款需要提交原支付订单的商户订单号和设置不同的退款单号。一笔退款失败后重新提交，要采用原来的退款单号。总退款金额不能超过用户实际支付金额。 
            ///3.接口提交成功后，还需要在微信商户后台由商户管理员审核退款
            /// </summary>
            /// <param name="appid">(必填) String(32) 微信分配的公众账号ID</param>
            /// <param name="mch_id">(必填) String(32) 微信支付分配的商户号</param>
            /// <param name="device_info"> String(32) 微信支付分配的终端设备号，商户自定义</param>
            /// <param name="nonce_str">(必填) 随机字符串 随机字符串，不长于32位。</param>
            /// <param name="transaction_id">String(32) 微信订单号 微信的订单号，优先使用 </param>
            /// <param name="out_trade_no">(transaction_id为空时必填) String(32) 商户订单号 transaction_id、out_trade_no二选一，如果同时存在优先级：transaction_id> out_trade_no </param>
            /// <param name="out_refund_no">(必填) String(32) 商户退款单号 商户系统内部的退款单号，商户系统内部唯一，同一退款单号多次请求只退一笔 </param>
            /// <param name="total_fee">(必填) int 总金额 订单总金额，单位为分，只能为整数。 </param>
            /// <param name="refund_fee">(必填) int  退款金额 退款总金额，订单总金额，单位为分，只能为整数</param>
            /// <param name="refund_fee_type">String(8) 货币种类 符合ISO 4217标准的三位字母代码，默认人民币：CNY</param>
            /// <param name="op_user_id">(必填) String(32) 操作员 操作员帐号, 默认为商户号mch_id </param>
            /// <param name="partnerKey">API密钥</param>
            /// <returns> 参见：http://pay.weixin.qq.com/wiki/doc/api/index.php?chapter=9_4 </returns>
            public static string Refund(string appid, string mch_id, string device_info, string nonce_str,
                                         string transaction_id, string out_trade_no, string out_refund_no,
                                         int total_fee, int refund_fee, string refund_fee_type, string op_user_id,
                                         string partnerKey)
            {
                var stringADict = new Dictionary<string, string>
                {
                    { "appid", appid },
                    { "mch_id", mch_id },
                    { "device_info", device_info },
                    { "nonce_str", nonce_str },
                    { "transaction_id", transaction_id },
                    { "out_trade_no", out_trade_no },
                    { "out_refund_no", out_refund_no },
                    { "total_fee", total_fee.ToString() },
                    { "refund_fee", refund_fee.ToString() },
                    { "refund_fee_type", refund_fee_type },
                    { "op_user_id", op_user_id }
                };

                var sign = PayUtil.Sign(stringADict, partnerKey);//生成签名字符串
                var postdata = PayUtil.GeneralPostdata(stringADict, sign);
                var url = "https://api.mch.weixin.qq.com/secapi/pay/refund";

                var result = NetnrCore.HttpTo.Post(url, postdata);
                return result;
            }
        }
    }
}
