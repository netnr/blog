using Netnr.WeChat.Helpers;
using System.Collections.Generic;

namespace Netnr.WeChat
{
    /// <summary>
    /// 
    /// </summary>
    public class Pay
    {
        /// <summary>
        /// 公共API => 统一下单
        /// 不需要证书 http://pay.weixin.qq.com/wiki/doc/api/index.php?chapter=9_1
        /// 应用场景 
        /// 除被扫支付场景以外，商户系统先调用该接口在微信支付服务后台生成预支付交易单，返回正确的预支付交易回话标识后再按扫码、JSAPI、APP等不同场景生成交易串调起支付。 
        /// </summary>
        /// <param name="appid">(必填) String(32) 微信分配的公众账号ID</param>
        /// <param name="mch_id">(必填) String(32) 微信支付分配的商户号</param>
        /// <param name="device_info"> String(32) 微信支付分配的终端设备号，商户自定义</param>
        /// <param name="nonce_str">(必填) String(32) 随机字符串,不长于32位</param>
        /// <param name="body">(必填) String(32) 商品描述 商品或支付单简要描</param>
        /// <param name="detail"> String(8192) 商品详情  商品名称明细列表</param>
        /// <param name="attach"> String(127) 附加数据，在查询API和支付通知中原样返回，该字段主要用于商户携带订单的自定义数据</param>
        /// <param name="out_trade_no">(必填) String(32) 商家订单ID,32个字符内、可包含字母, 其他说明见第4.2节商户订单号:http://pay.weixin.qq.com/wiki/doc/api/index.php?chapter=4_2 </param>
        /// <param name="fee_type">符合ISO 4217标准的三位字母代码，默认人民币：CNY，其他值列表详见第4.2节货币类型: http://pay.weixin.qq.com/wiki/doc/api/index.php?chapter=4_2 </param>
        /// <param name="total_fee">(必填) Int 订单总金额，只能为整数，详见第4.2节支付金额:http://pay.weixin.qq.com/wiki/doc/api/index.php?chapter=4_2 </param>
        /// <param name="spbill_create_ip">(必填) String(32)终端IP APP和网页支付提交用户端IP，Native支付填调用微信支付API的机器IP。</param>
        /// <param name="time_start">String(14) 订单生成时间，格式为yyyyMMddHHmmss，如2009年12月25日9点10分10秒表示为20091225091010。</param>
        /// <param name="time_expire">String(14) 订单失效时间，格式为yyyyMMddHHmmss，如2009年12月27日9点10分10秒表示为20091227091010。</param>
        /// <param name="goods_tag">String(32) 商品标记，代金券或立减优惠功能的参数，说明详见第10节代金券或立减优惠：http://pay.weixin.qq.com/wiki/doc/api/index.php?chapter=10_1 </param>
        /// <param name="notify_url">(必填) String(256)  接收微信支付异步通知回调地址 </param>
        /// <param name="trade_type">(必填) String(16) 交易类型，取值如下：JSAPI，NATIVE，APP，详细说明见第4.2节参数规定：http://pay.weixin.qq.com/wiki/doc/api/index.php?chapter=4_2 </param>
        /// <param name="product_id">(trade_type=NATIVE，此参数必传)String(32) 商品ID，trade_type=NATIVE，此参数必传。此id为二维码中包含的商品ID，商户自行定义。 </param>
        /// <param name="openid">(trade_type=JSAPI，此参数必传)String(128)用户标识，trade_type=JSAPI，此参数必传，用户在商户appid下的唯一标识。下单前需要调用【网页授权获取用户信息】接口获取到用户的Openid。 </param>
        /// <param name="partnerKey">API密钥</param>
        /// <param name="returnMsg"></param>
        /// <param name="xml"></param>
        /// <returns>返回json字符串，格式参见：http://pay.weixin.qq.com/wiki/doc/api/index.php?chapter=9_1 </returns>
        public static string UnifiedOrder(string appid, string mch_id, string device_info,
                                         string nonce_str, string body, string detail, string attach,
                                         string out_trade_no, string fee_type, int total_fee, string spbill_create_ip, string time_start, string time_expire, string goods_tag,
                                         string notify_url, string trade_type, string product_id, string openid,
                                         string partnerKey, out string returnMsg, out string xml
                                        )
        {
            var stringADict = new Dictionary<string, string>
            {
                { "appid", appid },
                { "mch_id", mch_id },
                { "device_info", device_info },
                { "nonce_str", nonce_str },
                { "body", body },
                { "attach", attach },
                { "out_trade_no", out_trade_no },
                { "fee_type", fee_type },
                { "total_fee", total_fee.ToString() },
                { "spbill_create_ip", spbill_create_ip },
                { "time_start", time_start },
                { "time_expire", time_expire },
                { "goods_tag", goods_tag },
                { "notify_url", notify_url },
                { "trade_type", trade_type },
                { "product_id", product_id },
                { "openid", openid }
            };
            var sign = PayUtil.Sign(stringADict, partnerKey);//生成签名字符串
            var postdata = PayUtil.GeneralPostdata(stringADict, sign);
            xml = postdata;
            var url = "https://api.mch.weixin.qq.com/pay/unifiedorder";
            var result = NetnrCore.HttpTo.Post(url, postdata);
            returnMsg = result;
            return result;
        }

        /// <summary>
        /// 公共API => 查询订单
        /// 不需要证书 
        /// http://pay.weixin.qq.com/wiki/doc/api/index.php?chapter=9_2
        /// 该接口提供所有微信支付订单的查询，商户可以通过该接口主动查询订单状态，完成下一步的业务逻辑。 
        /// 需要调用查询接口的情况： 
        ///◆　当商户后台、网络、服务器等出现异常，商户系统最终未接收到支付通知； 
        ///◆　调用支付接口后，返回系统错误或未知交易状态情况； 
        ///◆　调用被扫支付API，返回USERPAYING的状态； 
        ///◆　调用关单或撤销接口API之前，需确认支付状态； 
        /// </summary>
        /// <param name="appid">(必填) String(32) 微信分配的公众账号ID</param>
        /// <param name="mch_id">(必填) String(32) 微信支付分配的商户号</param>
        /// <param name="transaction_id">String(32) 微信订单号 微信的订单号，优先使用 </param>
        /// <param name="out_trade_no">(transaction_id为空时必填) String(32) 商户订单号 商户系统内部的订单号，当没提供transaction_id时需要传这个。 </param>
        /// <param name="nonce_str">随机字符串 随机字符串，不长于32位。</param>
        /// <param name="partnerKey">API密钥</param>
        /// <returns> 参见：http://pay.weixin.qq.com/wiki/doc/api/index.php?chapter=9_2 </returns>
        public static string OrderQuery(string appid, string mch_id, string transaction_id, string out_trade_no, string nonce_str, string partnerKey)
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
            var url = "https://api.mch.weixin.qq.com/pay/orderquery";

            var result = NetnrCore.HttpTo.Post(url, postdata);
            return result;
        }


        /// <summary>
        /// 公共API => 关闭订单
        /// http://pay.weixin.qq.com/wiki/doc/api/index.php?chapter=9_3
        /// 应用场景 
        /// 不需要证书 
        ///以下情况需要调用关单接口：商户订单支付失败需要生成新单号重新发起支付，要对原订单号调用关单，避免重复支付；系统下单后，用户支付超时，系统退出不再受理，避免用户继续，请调用关单接口。
        /// </summary>
        /// <param name="appid">(必填) String(32) 微信分配的公众账号ID</param>
        /// <param name="mch_id">(必填) String(32) 微信支付分配的商户号</param>
        /// <param name="transaction_id"></param>
        /// <param name="out_trade_no">(transaction_id为空时必填) String(32) 商户订单号 商户系统内部的订单号，当没提供transaction_id时需要传这个。 </param>
        /// <param name="nonce_str">随机字符串 随机字符串，不长于32位。</param>
        /// <param name="partnerKey">API密钥</param>
        /// <returns> 参见：http://pay.weixin.qq.com/wiki/doc/api/index.php?chapter=9_3 </returns>
        public static string CloseOrder(string appid, string mch_id, string transaction_id, string out_trade_no, string nonce_str, string partnerKey)
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
            var url = "https://api.mch.weixin.qq.com/pay/closeorder";

            var result = NetnrCore.HttpTo.Post(url, postdata);
            return result;
        }



        /// <summary>
        /// 公共API => 查询退款
        /// 不需要证书
        /// http://pay.weixin.qq.com/wiki/doc/api/index.php?chapter=9_5
        ///应用场景 
        ///提交退款申请后，通过调用该接口查询退款状态。退款有一定延时，用零钱支付的退款20分钟内到账，银行卡支付的退款3个工作日后重新查询退款状态。 
        /// </summary>
        /// <param name="appid">(必填) String(32) 微信分配的公众账号ID</param>
        /// <param name="mch_id">(必填) String(32) 微信支付分配的商户号</param>
        /// <param name="device_info"> String(32) 微信支付分配的终端设备号，商户自定义</param>
        /// <param name="nonce_str">(必填) 随机字符串 随机字符串，不长于32位。</param>
        /// <param name="transaction_id">String(32) 微信订单号 微信的订单号，优先使用 </param>
        /// <param name="out_trade_no">String(32) 商户订单号 transaction_id、out_trade_no二选一，如果同时存在优先级：transaction_id> out_trade_no </param>
        /// <param name="out_refund_no">String(32) 商户退款单号 商户系统内部的退款单号，商户系统内部唯一，同一退款单号多次请求只退一笔 </param>
        /// <param name="refund_id"> String(28) 微信退款单号  refund_id、out_refund_no、out_trade_no、transaction_id四个参数必填一个，如果同时存在优先级为： refund_id>out_refund_no>transaction_id>out_trade_no  </param>
        /// <param name="partnerKey">API密钥</param>
        /// <returns> 参见：http://pay.weixin.qq.com/wiki/doc/api/index.php?chapter=9_5 </returns>
        public static string RefundQuery(string appid, string mch_id, string device_info, string nonce_str,
                                     string transaction_id, string out_trade_no, string out_refund_no, string refund_id,
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
                { "refund_id", refund_id }
            };

            var sign = PayUtil.Sign(stringADict, partnerKey);//生成签名字符串
            var postdata = PayUtil.GeneralPostdata(stringADict, sign);
            var url = "https://api.mch.weixin.qq.com/pay/refundquery";

            var result = NetnrCore.HttpTo.Post(url, postdata);
            return result;
        }


        /// <summary>
        /// 公共API => 下载对账单
        /// 不需要证书
        /// http://pay.weixin.qq.com/wiki/doc/api/index.php?chapter=9_6
        ///应用场景 
        ///商户可以通过该接口下载历史交易清单。比如掉单、系统错误等导致商户侧和微信侧数据不一致，通过对账单核对后可校正支付状态。 
        ///注意： 
        ///1.微信侧未成功下单的交易不会出现在对账单中。支付成功后撤销的交易会出现在对账单中，跟原支付单订单号一致，bill_type为REVOKED； 
        ///2.微信在次日9点启动生成前一天的对账单，建议商户10点后再获取； 
        ///3.对账单中涉及金额的字段单位为“元”。 
        /// </summary>
        /// <param name="appid">(必填) String(32) 微信分配的公众账号ID</param>
        /// <param name="mch_id">(必填) String(32) 微信支付分配的商户号</param>
        /// <param name="device_info"> String(32) 微信支付分配的终端设备号，商户自定义</param>
        /// <param name="nonce_str">(必填) 随机字符串 随机字符串，不长于32位。</param>
        /// <param name="bill_date">String(8) 对账单日起 下载对账单的日期，格式：20140603 </param>
        /// <param name="bill_type">String(8) 账单类型  ALL，返回当日所有订单信息，默认值 SUCCESS，返回当日成功支付的订单； REFUND，返回当日退款订单； REVOKED，已撤销的订单 </param>
        /// <param name="partnerKey">API密钥</param>
        /// <returns> 
        /// 成功时，数据以文本表格的方式返回，第一行为表头，后面各行为对应的字段内容，字段内容跟查询订单或退款结果一致，具体字段说明可查阅相应接口。 
        /// 参见：http://pay.weixin.qq.com/wiki/doc/api/index.php?chapter=9_6 </returns>
        public static string RefundQuery(string appid, string mch_id, string device_info, string nonce_str,
                                     string bill_date, string bill_type,
                                     string partnerKey)
        {
            var stringADict = new Dictionary<string, string>
            {
                { "appid", appid },
                { "mch_id", mch_id },
                { "device_info", device_info },
                { "nonce_str", nonce_str },
                { "bill_date", bill_date },
                { "bill_type", bill_type }
            };

            var sign = PayUtil.Sign(stringADict, partnerKey);//生成签名字符串
            var postdata = PayUtil.GeneralPostdata(stringADict, sign);
            var url = "https://api.mch.weixin.qq.com/pay/refundquery";

            var result = NetnrCore.HttpTo.Post(url, postdata);
            return result;
        }
        
        /// <summary>
        /// 公共API => 测速上报
        /// 不需要证书
        /// http://pay.weixin.qq.com/wiki/doc/api/index.php?chapter=9_8
        ///应用场景 
        ///商户在调用微信支付提供的相关接口时，会得到微信支付返回的相关信息以及获得整个接口的响应时间。为提高整体的服务水平，
        ///协助商户一起提高服务质量，微信支付提供了相关接口调用耗时和返回信息的主动上报接口，微信支付可以根据商户侧上报的数据进一步优化网络部署，完善服务监控，和商户更好的协作为用户提供更好的业务体验。 
        /// </summary>
        /// <param name="appid">(必填) String(32) 微信分配的公众账号ID</param>
        /// <param name="mch_id">(必填) String(32) 微信支付分配的商户号</param>
        /// <param name="device_info"> String(32) 微信支付分配的终端设备号，商户自定义</param>
        /// <param name="nonce_str">(必填) 随机字符串 随机字符串，不长于32位。</param>
        /// <param name="interface_url">String(127) 接口URL  上报对应的接口的完整URL，类似： 
        ///https://api.mch.weixin.qq.com/pay/unifiedorder
        ///对于被扫支付，为更好的和商户共同分析一次业务行为的整体耗时情况，对于两种接入模式，请都在门店侧对一次被扫行为进行一次单独的整体上报，上报URL指定为： 
        ///https://api.mch.weixin.qq.com/pay/micropay/total
        ///关于两种接入模式具体可参考本文档章节：被扫支付商户接入模式
        ///其它接口调用仍然按照调用一次，上报一次来进行。 
        /// </param>
        /// <param name="execute_time_">Int  接口耗时   接口耗时情况，单位为毫秒 </param>
        /// <param name="return_code">String(16)  返回状态码   接口耗时情况，单位为毫秒 </param>
        /// <param name="return_msg">String(128)  返回信息    返回信息，如非空，为错误原因 :签名失败; 参数格式校验错误 </param>
        /// <param name="result_code">String(16)  业务结果    SUCCESS/FAIL </param>
        /// <param name="err_code">String(32)  错误代码   </param>
        /// <param name="err_code_des">String(128)  错误代码描述</param>
        /// <param name="out_trade_no">String(32)  商户订单号 商户系统内部的订单号,商户可以在上报时提供相关商户订单号方便微信支付更好的提高服务质量。 </param>
        /// <param name="user_ip">String(16)  访问接口IP   发起接口调用时的机器IP  </param>
        /// <param name="time">String(14)  商户上报时间 系统时间，格式为yyyyMMddHHmmss，如2009年12月27日9点10分10秒表示为20091227091010。 其他详见第4.2节时间规则：http://pay.weixin.qq.com/wiki/doc/api/index.php?chapter=4_2  </param>
        /// <param name="partnerKey">API密钥</param>
        /// <returns> 参见：http://pay.weixin.qq.com/wiki/doc/api/index.php?chapter=9_8 </returns>
        public static string RefundQuery(string appid, string mch_id, string device_info, string nonce_str,
                                     string interface_url, string execute_time_,
                                     string return_code, string return_msg, string result_code, string err_code,
                                     string err_code_des, string out_trade_no, string user_ip, string time,
                                     string partnerKey)
        {
            var stringADict = new Dictionary<string, string>
            {
                { "appid", appid },
                { "mch_id", mch_id },
                { "device_info", device_info },
                { "nonce_str", nonce_str },
                { "interface_url", interface_url },
                { "execute_time_", execute_time_ },
                { "return_code", return_code },
                { "return_msg", return_msg },
                { "result_code", result_code },
                { "err_code", err_code },
                { "err_code_des", err_code_des },
                { "out_trade_no", out_trade_no },
                { "user_ip", user_ip },
                { "time", time }
            };

            var sign = PayUtil.Sign(stringADict, partnerKey);//生成签名字符串
            var postdata = PayUtil.GeneralPostdata(stringADict, sign);
            var url = "https://api.mch.weixin.qq.com/pay/refundquery";

            var result = NetnrCore.HttpTo.Post(url, postdata);
            return result;
        }

        /// <summary>
        /// 提交被扫支付API
        /// http://pay.weixin.qq.com/wiki/doc/api/index.php?chapter=5_5
        /// 1.应用场景:收银员使用扫码设备读取微信用户刷卡授权码以后，二维码或条码信息传送至商户收银台，由商户收银台或者商户后台调用该接口发起支付。
        ///是否需要证书:不需要。
        /// <param name="appid">(必填) String(32) 微信分配的公众账号ID</param>
        /// <param name="mch_id">(必填) String(32) 微信支付分配的商户号</param>
        /// <param name="device_info"> String(32) 微信支付分配的终端设备号，商户自定义</param>
        /// <param name="nonce_str">(必填) String(32) 随机字符串,不长于32位</param>
        /// <param name="body">(必填) String(32) 商品描述 商品或支付单简要描</param>
        /// <param name="detail"> String(8192) 商品详情  商品名称明细列表</param>
        /// <param name="attach"> String(127) 附加数据，在查询API和支付通知中原样返回，该字段主要用于商户携带订单的自定义数据</param>
        /// <param name="out_trade_no">(必填) String(32) 商家订单ID,32个字符内、可包含字母, 其他说明见第4.2节商户订单号:http://pay.weixin.qq.com/wiki/doc/api/index.php?chapter=4_2 </param>
        /// <param name="fee_type">符合ISO 4217标准的三位字母代码，默认人民币：CNY，其他值列表详见第4.2节货币类型: http://pay.weixin.qq.com/wiki/doc/api/index.php?chapter=4_2 </param>
        /// <param name="total_fee">(必填) Int 订单总金额，只能为整数，详见第4.2节支付金额:http://pay.weixin.qq.com/wiki/doc/api/index.php?chapter=4_2 </param>
        /// <param name="spbill_create_ip">(必填) String(32)终端IP APP和网页支付提交用户端IP，Native支付填调用微信支付API的机器IP。</param>
        /// <param name="time_start">String(14) 订单生成时间，格式为yyyyMMddHHmmss，如2009年12月25日9点10分10秒表示为20091225091010。</param>
        /// <param name="time_expire">String(14) 订单失效时间，格式为yyyyMMddHHmmss，如2009年12月27日9点10分10秒表示为20091227091010。</param>
        /// <param name="goods_tag">String(32) 商品标记，代金券或立减优惠功能的参数，说明详见第10节代金券或立减优惠：http://pay.weixin.qq.com/wiki/doc/api/index.php?chapter=10_1 </param>
        /// <param name="auth_code">String(128) 授权码 扫码支付授权码，设备读取用户微信中的条码或者二维码信息 </param>
        /// <param name="partnerKey">API密钥</param>
        /// </summary>
        /// <returns>返回json字符串，格式参见：http://pay.weixin.qq.com/wiki/doc/api/index.php?chapter=5_5 </returns>
        public static string MicroPay(string appid, string mch_id, string device_info,
                                     string nonce_str, string body, string detail, string attach,
                                     string out_trade_no, string fee_type, int total_fee, string spbill_create_ip,
                                     string time_start, string time_expire, string goods_tag,
                                     string auth_code,
                                     string partnerKey)
        {
            var stringADict = new Dictionary<string, string>
            {
                { "appid", appid },
                { "mch_id", mch_id },
                { "device_info", device_info },
                { "nonce_str", nonce_str },
                { "body", body },
                { "attach", attach },
                { "out_trade_no", out_trade_no },
                { "fee_type", fee_type },
                { "total_fee", total_fee.ToString() },
                { "spbill_create_ip", spbill_create_ip },
                { "time_start", time_start },
                { "time_expire", time_expire },
                { "goods_tag", goods_tag },
                { "auth_code", auth_code }
            };
            var sign = PayUtil.Sign(stringADict, partnerKey);//生成签名字符串
            var postdata = PayUtil.GeneralPostdata(stringADict, sign);
            var url = "https://api.mch.weixin.qq.com/pay/micropay";

            var result = NetnrCore.HttpTo.Post(url, postdata);
            return result;
        }
    }
}
