using Netnr.WeChat.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netnr.WeChat
{
    /// <summary>
    /// 
    /// </summary>
    public class Card
    {
        /// <summary>
        /// 删除卡券
        /// 删除卡券接口允许商户删除任意一类卡券。删除卡券后，该卡券对应已生成的领取用二维码、添加到卡包JSAPI均会失效。
        /// 注意：如用户在商家删除卡券前已领取一张或多张该卡券依旧有效。即删除卡券不能删除已被用户领取，保存在微信客户端中的卡券。
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="card_id">卡券ID</param>
        /// <returns>{
        ///"errcode":0,
        ///"errmsg":"ok"
        ///}</returns>
        public static string Delete(string access_token, string card_id)
        {
            var url = string.Format("https://api.weixin.qq.com/card/delete?access_token={0}", access_token);
            var sb = new StringBuilder();
            sb.Append("{")
                .Append('"' + "card_id" + '"' + ":").Append(card_id)
                .Append("}");
            var result = NetnrCore.HttpTo.Post(url, sb.ToString());
            return result;
        }

        /// <summary>
        /// 查询code
        /// 
        /// 调用查询code接口可获取code的有效性（非自定义code），该code对应的用户openid、卡券有效期等信息。
        /// 自定义code（use_custom_code为true）的卡券调用接口时，post数据中需包含card_id，非自定义code不需上报。
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="code">要查询的序列号</param>
        /// <param name="card_id">要消耗序列号所述的card_id，生成券时use_custom_code填写true时必填。非自定义code不必填写。</param>
        /// <returns>{
        ///"errcode":0,
        ///"errmsg":"ok",
        ///"openid":"oFS7Fjl0WsZ9AMZqrI80nbIq8xrA",
        ///"card":{
        ///"card_id":"pFS7Fjg8kV1IdDz01r4SQwMkuCKc",
        ///"begin_time":1404205036,
        ///"end_time":1404205036,
        ///}
        ///}
        ///
        ///注：固定时长有效期会根据用户实际领取时间转换，如用户2013年10月1日领取，固定时长有效期为90天，即有效时间为2013年10月1日-12月29日有效。
        /// </returns>
        public static string Get(string access_token, string code, string card_id = "")
        {
            var url = string.Format("https://api.weixin.qq.com/card/get?access_token={0}", access_token);
            var sb = new StringBuilder();
            sb.Append("{")
              .Append('"' + "code" + '"' + ":").Append(code);
            if (!string.IsNullOrEmpty(card_id))
            {
                sb.Append('"' + "card_id" + '"' + ":").Append(card_id);
            }
            sb.Append("}");
            var result = NetnrCore.HttpTo.Post(url, sb.ToString());
            return result;
        }

        /// <summary>
        /// 批量查询卡列表
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="offset">查询卡列表的起始偏移量，从0开始，即offset:5是指从从列表里的第六个开始读取。</param>
        /// <param name="count">需要查询的卡片的数量（数量最大50）</param>
        /// <returns>{
        /// "errcode":0,
        /// "errmsg":"ok",
        /// "card_id_list":["ph_gmt7cUVrlRk8swPwx7aDyF-pg"],
        /// "total_num":1
        /// }</returns>
        public static string BatchGet(string access_token, int offset, int count)
        {
            var url = string.Format("https://api.weixin.qq.com/card/batchget?access_token={0}", access_token);
            var sb = new StringBuilder();
            sb.Append("{")
              .Append('"' + "offset" + '"' + ":").Append(offset)
              .Append('"' + "count" + '"' + ":").Append(count)
              .Append("}");
            var result = NetnrCore.HttpTo.Post(url, sb.ToString());
            return result;
        }

        /// <summary>
        /// 查询卡券详情
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="card_id">卡券ID</param>
        /// <returns>
        /// 返回结果示意
        ///｛"errcode":0,
        ///"errmsg":"ok",
        ///"card":{
        ///"card_type": "GROUPON",
        ///"groupon":{
        ///"base_info":{
        ///"status":1,
        ///"id":"p1Pj9jr90_SQRaVqYI239Ka1erkI",
        ///"logo_url":
        ///"http://www.supadmin.cn/uploads/allimg/120216/1_120216214725_1.jpg",
        ///"appid":"wx588def6b0089dd48",
        ///"code_type":"CODE_TYPE_TEXT",
        ///"brand_name":"海底捞",
        ///"title":"132元双人火锅套餐",
        ///"sub_title":"",
        ///"date_info":{
        ///"type":1,
        ///"begin_timestamp":1397577600,
        ///"end_timestamp":1399910400
        ///},
        ///"color":"#3373bb",
        ///"notice":"使用时向服务员出示此券",
        ///"service_phone":"020-88888888",
        ///"description":"不可与其他优惠同享\n如需团购券发票，请在消费时向商户提出\n店内均可使用，仅限堂食\n餐前不可打包，餐后未吃完，可打包\n本团购券不限人数，建议2人使用，超过建议人数须另收酱料费5元/位\n本单谢绝自带酒水饮料",
        ///     "use_limit":1,
        ///"get_limit":3,
        ///"can_share":true,
        ///"location_id_list": [123,12321,345345]
        ///"url_name_type":"URL_NAME_TYPE_RESERVATION",
        ///"custom_url":"http://www.qq.com",
        ///"source":"大众点评"
        ///"sku":{
        ///"quantity":0
        ///}
        ///},
        ///"deal_detail":"以下锅底2选1（有菌王锅、麻辣锅、大骨锅、番茄锅、清补凉锅、酸菜鱼锅可选）：\n大锅1份12元\n小锅2份16元\n以下菜品2选1\n特级肥牛1份30元\n洞庭鮰鱼卷1份20元\n其他\n鲜菇猪肉滑1份18元\n金针菇1份16元\n黑木耳1份9元\n娃娃菜1份8元\n冬瓜1份6元\n火锅面2个6元\n欢乐畅饮2位12元\n自助酱料2位10元",
        ///}
        ///}
        ///}
        ///}
        ///</returns>
        public static string Get(string access_token, string card_id)
        {
            var url = string.Format("https://api.weixin.qq.com/card/get?access_token={0}", access_token);
            var sb = new StringBuilder();
            sb.Append("{")
              .Append('"' + "card_id" + '"' + ":").Append(card_id)
              .Append("}");
            var result = NetnrCore.HttpTo.Post(url, sb.ToString());
            return result;
        }

        /// <summary>
        /// 查询卡券详情
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="new_card">
        /// {
        ///"card_id": "xxxxxxxxxxxxx",
        ///"member_card":{
        ///"base_info":{
        ///"logo_url":
        ///"http:\/\/www.supadmin.cn\/uploads\/allimg\/120216\/1_120216214725_1.jpg",
        ///"color":"Color010",
        ///"notice":"使用时向服务员出示此券",
        ///"service_phone":"020-88888888",
        ///"description":"不可与其他优惠同享\n如需团购券发票，请在消费时向商户提出\n店内均可使用，仅限堂食\n餐前不可打包，餐后未吃完，可打包\n本团购券不限人数，建议2人使用，超过建议人数须另收酱料费5元/位\n本单谢绝自带酒水饮料"
        ///"location_id_list": [123,12321,345345]
        ///},
        ///"bonus_cleared":"aaaaaaaaaaaaaa",
        ///"bonus_rules":"aaaaaaaaaaaaaa",
        ///"prerogative":""
        ///}
        ///}
        /// </param>
        /// <returns>
        /// {
        ///"errcode":0,
        ///"errmsg":"ok"
        ///}
        ///</returns>
        public static string Update(string access_token, object new_card)
        {
            var url = string.Format("https://api.weixin.qq.com/card/update?access_token={0}", access_token);
            var result = NetnrCore.HttpTo.Post(url, NetnrCore.ToJson(new_card));
            return result;
        }

        /// <summary>
        /// 库存修改接口
        /// 增减某张卡券的库存。
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="card_id">卡券ID</param>
        /// <param name="increase_stock_value">增加多少库存，可以不填或填0</param>
        /// <param name="reduce_stock_value">减少多少库存，可以不填或填0</param>
        /// <returns>
        /// {
        ///"errcode":0,
        ///"errmsg":"ok"
        ///}
        ///</returns>
        public static string ModIfyStock(string access_token, string card_id, int increase_stock_value, int reduce_stock_value)
        {
            var url = string.Format("https://api.weixin.qq.com/card//modifystock?access_token={0}", access_token);

            var sb = new StringBuilder();
            sb.Append("{")
              .Append('"' + "card_id" + '"' + ":").Append(card_id)
             .Append('"' + "increase_stock_value" + '"' + ":").Append(increase_stock_value)
             .Append('"' + "reduce_stock_value" + '"' + ":").Append(reduce_stock_value)
             .Append("}");
            var result = NetnrCore.HttpTo.Post(url, sb.ToString());

            return result;
        }

        /// <summary>
        /// 获取颜色列表接口
        /// 获得卡券的最新颜色列表，用于卡券创建。
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns>
        /// 返回数据结果示意：
        /// {
        ///"errcode":0,
        ///"errmsg":"ok",
        ///"colors":[
        ///{"name":"Color010","value":"#55bd47"},
        ///{"name":"Color020","value":"#10ad61"},
        ///{"name":"Color030","value":"#35a4de"},
        ///{"name":"Color040","value":"#3d78da"},
        ///{"name":"Color050","value":"#9058cb"},
        ///{"name":"Color060","value":"#de9c33"},
        ///{"name":"Color070","value":"#ebac16"},
        ///{"name":"Color080","value":"#f9861f"},
        ///{"name":"Color081","value":"#f08500"},
        ///{"name":"Color090","value":"#e75735"},
        ///{"name":"Color100","value":"#d54036"},
        ///{"name":"Color101","value":"#cf3e36"}
        ///]
        ///}</returns>
        public static string GetColors(string access_token)
        {
            var url = string.Format("https://api.weixin.qq.com/card/getcolors?access_token={0}", access_token);

            var result = NetnrCore.HttpTo.Get(url);

            return result;
        }

        /// <summary>
        /// 创建卡券
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="card">
        /// 数据示意：
        /// {"card":{
        /// "card_type":"GROUPON",
        /// "groupon":{
        /// "base_info":{
        /// "logo_url":
        /// "http:\/\/www.supadmin.cn\/uploads\/allimg\/120216\/1_120216214725_1.jpg",
        /// "brand_name":"海底捞",
        /// "code_type":"CODE_TYPE_TEXT",
        /// "title":"132元双人火锅套餐",
        /// "sub_title":"",
        /// "color":"Color010",
        /// "notice":"使用时向服务员出示此券",
        /// "service_phone":"020-88888888",
        /// "description":"不可与其他优惠同享\n如需团购券发票，请在消费时向商户提出\n店内均可
        ///                使用，仅限堂食\n餐前不可打包，餐后未吃完，可打包\n本团购券不限人数，建议2人使用，超过建议人
        ///                数须另收酱料费5元/位\n本单谢绝自带酒水饮料",
        /// "date_info":{
        /// "type":1,
        /// "begin_timestamp":1397577600,
        /// "end_timestamp":1422724261
        /// },
        /// "sku":{
        /// "quantity":50000000
        /// },
        ///  "get_limit":3,
        /// "use_custom_code":false,
        /// "bind_openid":false,
        /// "can_share":true,
        /// "can_give_friend":true,
        /// "location_id_list": [123,12321,345345],
        /// "url_name_type":"URL_NAME_TYPE_RESERVATION",
        /// "custom_url":"http://www.qq.com",
        /// "source":"大众点评"
        ///   },
        /// "deal_detail":"以下锅底2选1（有菌王锅、麻辣锅、大骨锅、番茄锅、清补凉锅、酸菜鱼锅可
        /// 选）：\n大锅1份12元\n小锅2份16元\n以下菜品2选1\n特级肥牛1份30元\n洞庭鮰鱼卷1份
        /// 20元\n其他\n鲜菇猪肉滑1份18元\n金针菇1份16元\n黑木耳1份9元\n娃娃菜1份8元\n冬
        /// 瓜1份6元\n火锅面2个6元\n欢乐畅饮2位12元\n自助酱料2位10元"}
        /// }
        /// }
        /// 具体参数意义，请参见官方文档。
        /// </param>
        /// <returns>
        /// {
        ///"errcode":0,
        ///"errmsg":"ok",
        ///"card_id":"p1Pj9jr90_SQRaVqYI239Ka1erkI"
        /// }</returns>
        public static string Create(string access_token, object card)
        {
            var url = string.Format("https://api.weixin.qq.com/card/create?access_token={0}", access_token);
            var result = NetnrCore.HttpTo.Post(url, NetnrCore.ToJson(card));
            return result;
        }

        /// <summary>
        /// 为检测跳转外链请求来自微信，会在URL参数里加上签名。
        /// </summary>
        /// <param name="appscret">appscret</param>
        /// <param name="encrypt_code">指定的卡券code码，只能被领一次。</param>
        /// <param name="card_id">创建卡券时获得的卡券ID</param>
        /// <returns></returns>
        public static string SignCustomUrl(string appscret, string encrypt_code, string card_id)
        {
            var stringADict = new Dictionary<string, string>
            {
                { "appscret", appscret },
                { "card_id", card_id },
                { "encrypt_code", encrypt_code }
            };
            var sb = new StringBuilder();
            foreach (var sA in stringADict.OrderBy(x => x.Key))//参数名ASCII码从小到大排序（字典序）；
            {
                if (string.IsNullOrEmpty(sA.Value)) continue;//参数的值为空不参与签名；
                sb.Append(sA.Key).Append("=").Append(sA.Value).Append("&");
            }
            var string1 = sb.ToString();
            string1 = string1.Remove(string1.Length - 1, 1);
            return Util.Sha1(string1, "UTF-8");//对stringSignTemp进行MD5运算，再将得到的字符串所有字符转换为大写，得到sign值signValue。 
        }

        /// <summary>
        /// 
        /// </summary>
        public class BoardingPass
        {
            /// <summary>
            /// 更新电影票
            /// 领取电影票后通过调用“更新电影票”接口update电影信息及用户选座信息。
            /// </summary>
            /// <param name="access_token"></param>
            /// <param name="tickect">
            /// {
            ///"code":"198374613512",
            ///"card_id":"p1Pj9jr90_SQRaVqYI239Ka1erkI",
            ///"passenger_name":"乘客姓名",//乘客姓名，上限为15个汉字。
            ///"class":"舱等",//舱等，如头等舱等，上限为5个汉字。
            ///"seat":"座位号",//乘客座位号。
            ///"etkt_bnr":"电子客票号",//电子客票号，上限为14个数字
            ///"qrcode_data":"二维码数据",//乘客用于值机的二维码字符串，微信会通过此数据为用户生成值机用的二维码。
            ///"is_cancel ":false//填写true或false。true代表取消，如填写true上述字段（如calss等）均不做判断，机票返回未值机状态，乘客可重新值机。默认填写false
            ///   }
            /// </param>
            /// <returns>
            /// {
            ///"errcode":0,
            ///"errmsg":"ok"
            ///}
            ///</returns>
            public static string CheckIn(string access_token, object tickect)
            {
                var url = string.Format("https://api.weixin.qq.com/card/boardingpass/checkin?access_token={0}", access_token);
                var result = NetnrCore.HttpTo.Post(url, NetnrCore.ToJson(tickect));
                return result;
            }
        }

        /// <summary>
        /// 卡券
        /// </summary>
        public class Code
        {
            /// <summary>
            /// 消耗code
            /// 消耗code接口是核销卡券的唯一接口，仅支持核销有效期内的卡券，否则会返回错误码invalidtime。
            /// 自定义code（use_custom_code为true）的优惠券，在code被核销时，必须调用此接口。用于将用户客户端的code状态变更。
            /// 自定义code的卡券调用接口时，post数据中需包含card_id，非自定义code不需上报。
            /// </summary>
            /// <param name="access_token"></param>
            /// <param name="code">要消耗序列号，创建卡券时use_custom_code填写true时必填。非自定义code不必填写。</param>
            /// <param name="card_id">卡券ID</param>
            /// <returns>
            /// {
            ///"errcode":0,
            ///"errmsg":"ok",
            ///"card":{"card_id":"pFS7Fjg8kV1IdDz01r4SQwMkuCKc"},
            ///"openid":"oFS7Fjl0WsZ9AMZqrI80nbIq8xrA"
            ///}
            ///
            /// 错误码，0：正常，40099：该code已被核销
            ///</returns>
            public static string Consume(string access_token, string code, string card_id)
            {
                var url = string.Format("https://api.weixin.qq.com/card/code/consume?access_token={0}", access_token);

                var sb = new StringBuilder();
                sb.Append("{")
                    .Append('"' + "code" + '"' + ":").Append(code)
                    .Append('"' + "card_id" + '"' + ":").Append(card_id)
                    .Append("}");
                var result = NetnrCore.HttpTo.Post(url, sb.ToString());

                return result;
            }

            /// <summary>
            /// 签名算法
            /// </summary>
            /// <param name="api_ticket">api_ticket</param>
            /// <param name="app_id">公众号appID</param>
            /// <param name="location_id">门店信息</param>
            /// <param name="time_stamp">时间戳</param>
            /// <param name="nonce_str">随机字符串</param>
            ///  <param name="card_id">生成卡券时获得的card_id</param>
            ///  <param name="card_type">卡券类型</param>
            /// <returns>
            /// {
            /// "err_msg":"choose_card:ok",//choose_card:ok选取卡券成功
            /// "choose_card_info":[
            /// {
            /// "card_id":"p3G6It08WKRgR0hyV3hHVb6pxrPQ",
            /// "encrypt_code":"XXIzTtMqCxwOaawoE91+VJdsFmv7b8g0VZIZkqf4GWA60Fzpc8ksZ/5ZZ0DVkXdE"
            /// ]
            /// }
            /// }
            /// </returns>
            public static string GetSignature(string api_ticket, string app_id, string location_id, string time_stamp, string nonce_str, string card_id, string card_type)
            {
                var stringADict = new Dictionary<string, string>
                {
                    { "api_ticket", api_ticket },
                    { "app_id", app_id },
                    { "location_id", location_id },
                    { "timestamp", time_stamp },
                    { "nonce_str", nonce_str },
                    { "card_id", card_id },
                    { "card_type", card_type }
                };
                var string1Builder = new StringBuilder();
                foreach (var va in stringADict.OrderBy(x => x.Value))//将pi_ticket、app_id、location_id、times_tamp、nonce_str、card_id、card_type的value值进行字符串的字典序排序。
                {
                    string1Builder.Append(va.Value);
                }
                var signature = Util.Sha1(string1Builder.ToString());
                return signature;
            }

            /// <summary>
            /// 签名算法
            /// </summary>
            /// <param name="api_ticket">api_ticket</param>
            /// <param name="card_id">生成卡券时获得的card_id</param>
            /// <param name="timestamp">时间戳，
            /// signature中的timestamp和card_ext中的timestamp必须保持一致。
            /// 商户生成从1970年1月1日00:00:00至今的秒数,即当前的时间,且最终需要转换为字符串形式;由商户生成后传入。</param>
            /// <param name="code">指定的卡券code码，只能被领一次。use_custom_code字段为true的卡券必须填写，非自定义code不必填写。</param>
            /// <param name="openid">指定领取者的openid，只有该用户能领取。bind_openid字段为true的卡券必须填写，非自定义code不必填写。</param>
            ///  <param name="balance">红包余额，以分为单位。红包类型（LUCKY_MONEY）必填、其他卡券类型不必填写。</param>
            /// <returns></returns>
            public static string GetSignature(string api_ticket, string card_id, long timestamp, string code, string openid, string balance)
            {
                var stringADict = new Dictionary<string, string>
                {
                    { "api_ticket", api_ticket },
                    { "card_id", card_id },
                    { "timestamp", timestamp.ToString() },
                    { "code", code },
                    { "openid", openid },
                    { "balance", balance }
                };
                var string1Builder = new StringBuilder();
                foreach (var va in stringADict.OrderBy(x => x.Value))//将api_ticket、timestamp、card_id、code、openid、balance的value值进行字符串的字典序排序。
                {
                    string1Builder.Append(va.Value);
                }
                var signature = Util.Sha1(string1Builder.ToString());
                return signature;
            }

            /// <summary>
            /// code解码接口
            /// code解码接口支持两种场景：
            ///1.商家获取choos_card_info后，将card_id和encrypt_code字段通过解码接口，获取真实code。
            ///2.卡券内跳转外链的签名中会对code进行加密处理，通过调用解码接口获取真实code。
            /// </summary>
            /// <param name="access_token"></param>
            /// <param name="encrypt_code">通过choose_card_info获取的加密字符串</param>
            /// <returns>
            /// {
            ///"errcode":0,
            ///"errmsg":"ok",
            ///"code":"751234212312"
            ///}
            /// </returns>
            public static string Decrypt(string access_token, string encrypt_code)
            {
                var url = string.Format("https://api.weixin.qq.com/card/code/decrypt?access_token={0}", access_token);

                var sb = new StringBuilder();
                sb.Append("{")
                    .Append('"' + "encrypt_code" + '"' + ":").Append(encrypt_code)
                    .Append("}");
                var result = NetnrCore.HttpTo.Post(url, sb.ToString());

                return result;
            }

            /// <summary>
            /// 更改code
            /// 为确保转赠后的安全性，微信允许自定义code的商户对已下发的code进行更改。
            /// 注：为避免用户疑惑，建议仅在发生转赠行为后（发生转赠后，微信会通过事件推送的方式告知商户被转赠的卡券code）对用户的code进行更改。
            /// </summary>
            /// <param name="access_token"></param>
            /// <param name="card_id">卡券ID</param>
            /// <param name="code">卡券的code编码</param>
            /// <param name="new_code">新的卡券code编码</param>
            /// <returns>
            /// {
            ///"errcode":0,
            ///"errmsg":"ok"
            ///}</returns>
            public static string Update(string access_token, string code, string card_id, string new_code)
            {
                var url = string.Format("https://api.weixin.qq.com/card/code/update?access_token={0}", access_token);

                var sb = new StringBuilder();
                sb.Append("{")
                  .Append('"' + "code" + '"' + ":").Append(code)
                  .Append('"' + "card_id" + '"' + ":").Append(card_id)
                  .Append('"' + "new_code" + '"' + ":").Append(new_code)
                  .Append("}");
                var result = NetnrCore.HttpTo.Post(url, sb.ToString());

                return result;
            }

            /// <summary>
            /// 设置卡券失效接口
            /// 为满足改票、退款等异常情况，可调用卡券失效接口将用户的卡券设置为失效状态。
            /// 注：设置卡券失效的操作不可逆，即无法将设置为失效的卡券调回有效状态，商家须慎重调用该接口。
            /// </summary>
            /// <param name="access_token"></param>
            /// <param name="code">需要设置为失效的code</param>
            /// <param name="card_id">自定义code的卡券必填。非自定义code的卡券不填。</param>
            /// <returns>
            /// {
            ///"errcode":0,
            ///"errmsg":"ok"
            ///}
            ///</returns>
            public static string Unavailable(string access_token, string code, string card_id = "")
            {
                var url = string.Format("https://api.weixin.qq.com/card/code/unavailable?access_token={0}", access_token);

                var sb = new StringBuilder();
                sb.Append("{")
                  .Append('"' + "code" + '"' + ":").Append(code);
                if (!string.IsNullOrEmpty(card_id))
                {
                    sb.Append('"' + "card_id" + '"' + ":").Append(card_id);
                }
                sb.Append("}");
                var result = NetnrCore.HttpTo.Post(url, sb.ToString());

                return result;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Location
    {
        /// <summary>
        /// 批量导入门店信息
        /// 接口说明
        ///支持商户调用该接口批量导入/新建门店信息，获取门店ID。
        ///注：通过该接口导入的门店信息将进入门店审核流程，审核期间可正常使用。若导入的
        ///门店信息未通过审核，则会被剔除出门店列表。
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="location_list">门店列表
        /// 数据示意：
        /// {"location_list":[
        ///{
        ///"business_name":"麦当劳",
        ///"branch_name":"赤岗店",
        ///"province":"广东省",
        ///"city":"广州市",
        ///"district":"海珠区",
        ///"address":"中国广东省广州市海珠区艺苑路11号",
        ///"telephone":"020-89772059",
        ///"category":"房产小区",
        ///"longitude":"115.32375",
        ///"latitude":"25.097486"
        ///},
        ///{
        ///"business_name":"麦当劳",
        ///"branch_name":"珠江店",
        ///"province":"广东省",
        ///"city":"广州市",
        ///"district":"海珠区",
        ///"address":"中国广东省广州市海珠区艺苑路12号",
        ///"telephone":"020-89772059",
        ///"category":"房产小区",
        ///"longitude":"113.32375",
        ///"latitude":"23.097486"
        ///}]}</param>
        /// <returns>
        /// {
        /// "errcode":0,
        /// "errmsg":"ok",
        /// "location_id_list":[271262077,-1]
        ///}
        ///其中location_id_list中的 -1 表示失败
        /// </returns>
        public static string BatchAdd(string access_token, object location_list)
        {
            var url = string.Format("https://api.weixin.qq.com/card/location/batchadd?access_token={0}", access_token);
            var result = NetnrCore.HttpTo.Post(url, NetnrCore.ToJson(location_list));
            return result;
        }
        /// <summary>
        /// 拉取门店列表
        /// 获取在公众平台上申请创建及API导入的门店列表，用于创建卡券
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="offset">偏移量，0开始</param>
        /// <param name="count">拉取数量,为0时默认拉取全部门店。</param>
        /// <returns>
        /// 返回数据示意：
        /// { "errcode":0,
        ///"errmsg":"ok",
        ///"location_list":[
        ///{
        ///"location_id":“493”,
        ///"business_name":"steventaohome",
        ///"phone":"020-12345678",
        ///"address":"广东省广州市番禺区广东省广州市番禺区南浦大道",
        ///"longitude":113.280212402,
        ///"latitude":23.0350666046
        ///},
        ///{
        ///"location_id":“468”,
        ///"business_name":"TIT创意园B4",
        ///"phone":"020-12345678",
        ///"address":"广东省广州市海珠区",
        ///"longitude":113.325248718,
        ///"latitude":23.1008300781
        ///}
        ///],
        ///"count":2
        ///}
        /// </returns>
        public static string BatchGet(string access_token, int offset, int count)
        {
            var url = string.Format("https://api.weixin.qq.com/card/location/batchget??access_token={0}", access_token);

            var sb = new StringBuilder();
            sb.Append("{")
                .Append('"' + "offset" + '"' + ":").Append(offset)
                .Append('"' + "count" + '"' + ":").Append(count)
                .Append("}");
            var result = NetnrCore.HttpTo.Post(url, sb.ToString());

            return result;
        }
    }

    /// <summary>
    /// 特殊卡票接口=> 红包
    /// 商户调用接口创建会员卡获取card_id，并将会员卡下发给用户，用户领取后需激活/绑定update会员卡编号及积分信息。会员卡暂不支持转赠。
    /// </summary>
    public class LuckyMoney
    {
        /// <summary>
        /// 更新红包金额
        /// 支持领取红包后通过调用“更新红包”接口update红包余额。
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="code">红包的序列号</param>
        /// <param name="card_id">自定义code的卡券必填。非自定义code可不填。</param>
        /// <param name="balance">红包余额</param>
        /// <returns>
        /// {
        ///"errcode":0,
        ///"errmsg":"ok"
        ///}
        ///</returns>
        public static string UpdateUserBalance(string access_token, string code, string card_id, int balance)
        {
            var url = string.Format("https://api.weixin.qq.com/card/luckymoney/updateuserbalance?access_token={0}", access_token);

            var sb = new StringBuilder();
            sb.Append("{")
              .Append('"' + "code" + '"' + ":").Append(code)
              .Append('"' + "card_id" + '"' + ":").Append(card_id)
              .Append('"' + "balance" + '"' + ":").Append(balance)
              .Append("}");
            var result = NetnrCore.HttpTo.Post(url, sb.ToString());

            return result;
        }
    }

    /// <summary>
    /// 特殊卡票接口=> 会议门票
    /// 商户调用接口创建会员卡获取card_id，并将会员卡下发给用户，用户领取后需激活/绑定update会员卡编号及积分信息。会员卡暂不支持转赠。
    /// </summary>
    public class MeetingTicket
    {
        /// <summary>
        /// 更新会议门票接口
        /// 支持调用“更新会议门票”接口update入场时间、区域、座位等信息。
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="tickect">
        /// {
        /// "code":"717523732898",
        /// "card_id":"pXch-jvdwkJjY7evUFV-sGsoMl7A",
        /// "zone":"C区",
        /// "entrance": "东北门",
        /// "seat_number": "2排15号"
        /// }
        /// </param>
        /// <returns>
        /// {
        ///"errcode":0,
        ///"errmsg":"ok"
        ///}
        ///</returns>
        public static string UpdateUser(string access_token, object tickect)
        {
            var url = string.Format("https://api.weixin.qq.com/card/meetingticket/updateuser?access_token={0}", access_token);

            var result = NetnrCore.HttpTo.Post(url, NetnrCore.ToJson(tickect));

            return result;
        }
    }

    /// <summary>
    /// 特殊卡票接口=> 会员卡
    /// 商户调用接口创建会员卡获取card_id，并将会员卡下发给用户，用户领取后需激活/绑定update会员卡编号及积分信息。会员卡暂不支持转赠。
    /// </summary>
    public class MemberCard
    {
        /// <summary>
        /// 激活/绑定会员卡支持以下两种方式：
        ///1.用户点击卡券内“bind_old_card_url”、“activate_url”跳转商户自定义的H5页面，填写相关身份认证信息后，商户调用接口，完成激活/绑定。
        ///2.商户已知用户身份或无需进行绑定等操作，用户领取会员卡后，商户后台即时调用“激活/绑定会员卡”接口update会员卡编号及积分信息。
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="memeber_card">
        /// {
        ///"init_bonus": 100,
        ///"init_balance": 200,
        ///"membership_number": "AAA00000001",
        ///"code":"12312313",
        ///"card_id":"xxxx_card_id"
        ///}
        ///或
        ///{
        ///"bonus": “www.xxxx.com”,
        ///"balance":“www.xxxx.com”,
        ///"membership_number": "AAA00000001",
        ///"code":"12312313",
        ///"card_id":"xxxx_card_id"
        ///}
        /// </param>
        /// <returns>
        /// {
        ///"errcode":0,
        ///"errmsg":"ok"
        ///}
        ///</returns>
        public static string Activate(string access_token, object memeber_card)
        {
            var url = string.Format("https://api.weixin.qq.com/card/membercard/activate?access_token={0}", access_token);

            var result = NetnrCore.HttpTo.Post(url, NetnrCore.ToJson(memeber_card));

            return result;
        }

        /// <summary>
        /// 会员卡交易
        /// 会员卡交易后每次积分及余额变更需通过接口通知微信，便于后续消息通知及其他扩展功能。
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="code">要消耗的序列号</param>
        /// <param name="add_bonus">需要变更的积分，扣除积分用“-“表示。</param>
        /// <param name="record_bonus">商家自定义积分消耗记录，不超过14个汉字。</param>
        /// <param name="add_balance">需要变更的余额，扣除金额用“-”表示。单位为分</param>
        /// <param name="record_balance">商家自定义金额消耗记录，不超过14个汉字。</param>
        /// <param name="card_id">要消耗序列号所述的card_id。自定义code的会员卡必填。</param>
        /// <returns>
        /// {
        ///"errcode":0,
        ///"errmsg":"ok",
        ///"result_bonus":100,
        ///"result_balance":200
        ///"openid":"oFS7Fjl0WsZ9AMZqrI80nbIq8xrA"
        ///}
        /// </returns>
        public static string UpdateUser(string access_token, string code, int add_bonus, string record_bonus, int add_balance, string record_balance, string card_id)
        {
            var url = string.Format("https://api.weixin.qq.com/card/membercard/updateuser?access_token={0}", access_token);

            var sb = new StringBuilder();
            sb.Append("{")
              .Append('"' + "code" + '"' + ":").Append(code)
              .Append('"' + "card_id" + '"' + ":").Append(card_id)
              .Append('"' + "record_bonus" + '"' + ":").Append(record_bonus)
              .Append('"' + "add_bonus" + '"' + ":").Append(add_bonus)
              .Append('"' + "add_balance" + '"' + ":").Append(add_balance)
              .Append('"' + "record_balance" + '"' + ":").Append(record_balance)
              .Append("}");
            var result = NetnrCore.HttpTo.Post(url, sb.ToString());

            return result;
        }
    }

    /// <summary>
    /// 特殊卡票接口=> 电影票
    /// 电影票券主要分为以下两种：
    /// 1、电影票兑换券，归属于团购券。
    /// 2、选座电影票，在购买时需要选定电影、场次、座位，具备较强的时效性和特殊性，此类电影票券即文档中的电影票。
    /// 使用场景：用户点击商户H5页面提供的JSAPI（添加到卡包JSAPI）后，商户根据用户
    /// 电影票信息，调用接口创建卡券，获取card_id后，将带有唯一code的电影票下发给用户，
    /// 用户领取后通过接口（更新电影票）update用户选座信息。
    /// </summary>
    public class MovieTicket
    {
        /// <summary>
        /// 更新电影票
        /// 领取电影票后通过调用“更新电影票”接口update电影信息及用户选座信息。
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="tickect">
        /// {
        ///"code": "277217129962",
        ///"card_id":"p1Pj9jr90_SQRaVqYI239Ka1erkI",
        ///"ticket_class":"4D",
        ///"show_time":1408493192,
        ///"duration"：120,
        ///"screening_room":"5号影厅",
        ///"seat_number":["5排14号", "5排15号"]
        ///   }
        /// </param>
        /// <returns>
        /// {
        ///"errcode":0,
        ///"errmsg":"ok"
        ///}
        ///</returns>
        public static string UpdateUser(string access_token, object tickect)
        {
            var url = string.Format("https://api.weixin.qq.com/card/movieticket/updateuser?access_token={0}", access_token);

            var result = NetnrCore.HttpTo.Post(url, NetnrCore.ToJson(tickect));

            return result;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ParameterQrcodeType
    {
        /// <summary>
        /// 
        /// </summary>
        QR_LIMIT_SCENE = 1,
        /// <summary>
        /// 
        /// </summary>
        QR_SCENE = 2
    }

    /// <summary>
    /// 
    /// </summary>
    public class QrCode
    {
        /// <summary>
        /// 创建二维码
        /// 创建卡券后，商户可通过接口生成一张卡券二维码供用户扫码后添加卡券到卡包。
        ///自定义code的卡券调用接口时，post数据中需指定code，非自定义code不需指定，
        ///指定openid同理。指定后的二维码只能被扫描领取一次。
        ///
        /// 获取二维码ticket后，开发者可用ticket换取二维码图片详情
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="action">
        /// 数据示意：
        /// {
        /// "action_name":"QR_CARD",
        /// "action_info":{
        /// "card":{
        /// "card_id":"pFS7Fjg8kV1IdDz01r4SQwMkuCKc",
        /// "code":"198374613512",
        /// "openid":"oFS7Fjl0WsZ9AMZqrI80nbIq8xrA",
        /// "expire_seconds":"1800"，
        /// "is_unique_code":false,
        /// "outer_id": 1
        /// }
        /// }
        /// }</param>
        /// <returns>二维码图片地址</returns>
        public static string Create(string access_token, object action)
        {
            var url = string.Format("https://api.weixin.qq.com/card/qrcode/create?access_token={0}", access_token);
            var result = NetnrCore.HttpTo.Post(url, NetnrCore.ToJson(action));
            var ticket = JObject.Parse(result)["ticket"].ToString();
            return string.Format("https://mp.weixin.qq.com/cgi-bin/showqrcode?ticket={0}", ticket);
        }

        /// <summary>
        /// 生成带参数的二维码
        /// </summary>
        /// <param name="token"></param>
        /// <param name="type">二维码类型，QR_SCENE为临时,QR_LIMIT_SCENE为永久</param>
        /// <param name="scene_id">场景值ID，临时二维码时为32位非0整型，永久二维码时最大值为100000（目前参数只支持1--100000）</param>
        /// <param name="days"></param>
        /// <returns></returns>
        public static string Create(string token, ParameterQrcodeType type, int scene_id, int days)
        {
            var content = new StringBuilder();
            content.Append("{");
            var action_name = "QR_LIMIT_SCENE";
            if (type == ParameterQrcodeType.QR_SCENE)
            {
                action_name = "QR_SCENE";
                content.Append('"' + "expire_seconds" + '"' + ":").Append(new TimeSpan(days, 0, 0, 0, 0).TotalSeconds).Append(",");
            }
            content
            .Append('"' + "action_name" + '"' + ": " + '"' + action_name + '"' + ",")
            .Append('"' + "action_info" + '"' + ": " + "{" + '"' + "scene" + '"' + ":{" + '"' + "scene_id" + '"' + ":" + scene_id.ToString() + "}}}");

            var result = NetnrCore.HttpTo.Post(
                string.Format("https://api.weixin.qq.com/cgi-bin/qrcode/create?access_token={0}", token),
                content.ToString());
            return result;
        }
    }


    /// <summary>
    /// 设置测试用户白名单
    /// </summary>
    public class TestWhiteList
    {
        /// <summary>
        /// 设置测试用户白名单
        /// 由于卡券有审核要求，为方便公众号调试，可以设置一些测试帐号，这些帐号可领取未通过审核的卡券，体验整个流程。
        /// 注：同时支持“openid”、“username”两种字段设置白名单，总数上限为10个。
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="testwhitelist">
        /// {
        ///"openid":[ //测试的openid列表
        ///"o1Pj9jmZvwSyyyyyyBa4aULW2mA",
        ///"o1Pj9jmZvxxxxxxxxxULW2mA"
        ///],
        ///"username":[ //测试的微信号列表
        ///"afdvvf",
        ///"abcd"
        ///]
        ///}
        /// </param>
        /// <returns></returns>
        public static string Set(string access_token, object testwhitelist)
        {
            var url = string.Format("https://api.weixin.qq.com/card/testwhitelist/set?access_token={0}", access_token);
            var result = NetnrCore.HttpTo.Post(url, NetnrCore.ToJson(testwhitelist));

            return result;
        }
    }
}
