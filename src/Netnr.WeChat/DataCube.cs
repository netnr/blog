using System;
using System.Text;

namespace Netnr.WeChat
{
    /// <summary>
    /// 数据统计接口=>接口分析数据接口
    /// </summary>
    public class DataCube
    {
        /// <summary>
        /// 获取接口分析数据
        /// 最大时间跨度：30
        /// begin_date和end_date的差值需小于“最大时间跨度”（比如最大时间跨度为1时，begin_date和end_date的差值只能为0，才能小于1），否则会报错
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="begin_date">获取数据的起始日期，begin_date和end_date的差值需小于“最大时间跨度”（比如最大时间跨度为1时，begin_date和end_date的差值只能为0，才能小于1），否则会报错</param>
        /// <param name="end_date">获取数据的结束日期，end_date允许设置的最大值为昨日</param>
        /// <returns></returns>
        public static string GetInterfaceSummary(string access_token, DateTime begin_date, DateTime end_date)
        {
            var url = string.Format("https://api.weixin.qq.com/datacube/getinterfacesummary?access_token={0}", access_token);
            var builder = new StringBuilder();
            builder
                .Append("{")
                .Append('"' + "begin_date" + '"' + ":").Append(begin_date.ToString("yyyy-MM-dd")).Append(",")
                .Append('"' + "end_date" + '"' + ":").Append(end_date.ToString("yyyy-MM-dd"))
                .Append("}");

            var result = NetnrCore.HttpTo.Post(url, builder.ToString());
            return result;
        }

        /// <summary>
        /// 获取接口分析分时数据
        /// 最大时间跨度：1
        /// begin_date和end_date的差值需小于“最大时间跨度”（比如最大时间跨度为1时，begin_date和end_date的差值只能为0，才能小于1），否则会报错
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="begin_date">获取数据的起始日期，begin_date和end_date的差值需小于“最大时间跨度”（比如最大时间跨度为1时，begin_date和end_date的差值只能为0，才能小于1），否则会报错</param>
        /// <param name="end_date">获取数据的结束日期，end_date允许设置的最大值为昨日</param>
        /// <returns></returns>
        public static string GetInterfaceSummaryHour(string access_token, DateTime begin_date, DateTime end_date)
        {
            var url = string.Format("https://api.weixin.qq.com/datacube/getinterfacesummaryhour?access_token={0}", access_token);
            var builder = new StringBuilder();
            builder
                .Append("{")
                .Append('"' + "begin_date" + '"' + ":").Append(begin_date.ToString("yyyy-MM-dd")).Append(",")
                .Append('"' + "end_date" + '"' + ":").Append(end_date.ToString("yyyy-MM-dd"))
                .Append("}");

            var result = NetnrCore.HttpTo.Post(url, builder.ToString());
            return result;
        }

        /// <summary>
        /// 获取消息发送概况数据
        /// 最大时间跨度：7
        /// begin_date和end_date的差值需小于“最大时间跨度”（比如最大时间跨度为1时，begin_date和end_date的差值只能为0，才能小于1），否则会报错
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="begin_date">获取数据的起始日期，begin_date和end_date的差值需小于“最大时间跨度”（比如最大时间跨度为1时，begin_date和end_date的差值只能为0，才能小于1），否则会报错</param>
        /// <param name="end_date">获取数据的结束日期，end_date允许设置的最大值为昨日</param>
        /// <returns></returns>
        public static string GetUpStreamMsg(string access_token, DateTime begin_date, DateTime end_date)
        {
            var url = string.Format("https://api.weixin.qq.com/datacube/getupstreammsg?access_token={0}", access_token);
            var builder = new StringBuilder();
            builder
                .Append("{")
                .Append('"' + "begin_date" + '"' + ":").Append(begin_date.ToString("yyyy-MM-dd")).Append(",")
                .Append('"' + "end_date" + '"' + ":").Append(end_date.ToString("yyyy-MM-dd"))
                .Append("}");

            var result = NetnrCore.HttpTo.Post(url, builder.ToString());
            return result;
        }

        /// <summary>
        /// 获取消息分送分时数据
        /// 最大时间跨度：1
        /// begin_date和end_date的差值需小于“最大时间跨度”（比如最大时间跨度为1时，begin_date和end_date的差值只能为0，才能小于1），否则会报错
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="begin_date">获取数据的起始日期，begin_date和end_date的差值需小于“最大时间跨度”（比如最大时间跨度为1时，begin_date和end_date的差值只能为0，才能小于1），否则会报错</param>
        /// <param name="end_date">获取数据的结束日期，end_date允许设置的最大值为昨日</param>
        /// <returns></returns>
        public static string GetUpStreamMsgHour(string access_token, DateTime begin_date, DateTime end_date)
        {
            var url = string.Format("https://api.weixin.qq.com/datacube/getupstreammsghour?access_token={0}", access_token);
            var builder = new StringBuilder();
            builder
                .Append("{")
                .Append('"' + "begin_date" + '"' + ":").Append(begin_date.ToString("yyyy-MM-dd")).Append(",")
                .Append('"' + "end_date" + '"' + ":").Append(end_date.ToString("yyyy-MM-dd"))
                .Append("}");

            var result = NetnrCore.HttpTo.Post(url, builder.ToString());
            return result;
        }

        /// <summary>
        /// 获取消息发送周数据
        /// 最大时间跨度：30
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="begin_date">获取数据的起始日期，begin_date和end_date的差值需小于“最大时间跨度”（比如最大时间跨度为1时，begin_date和end_date的差值只能为0，才能小于1），否则会报错</param>
        /// <param name="end_date">获取数据的结束日期，end_date允许设置的最大值为昨日</param>
        /// <returns></returns>
        public static string GetUpStreamMsgWeek(string access_token, DateTime begin_date, DateTime end_date)
        {
            var url = string.Format("https://api.weixin.qq.com/datacube/getupstreammsgweek?access_token={0}", access_token);
            var builder = new StringBuilder();
            builder
                .Append("{")
                .Append('"' + "begin_date" + '"' + ":").Append(begin_date.ToString("yyyy-MM-dd")).Append(",")
                .Append('"' + "end_date" + '"' + ":").Append(end_date.ToString("yyyy-MM-dd"))
                .Append("}");

            var result = NetnrCore.HttpTo.Post(url, builder.ToString());
            return result;
        }

        /// <summary>
        /// 获取消息发送月数据
        /// 最大时间跨度：30
        /// begin_date和end_date的差值需小于“最大时间跨度”（比如最大时间跨度为1时，begin_date和end_date的差值只能为0，才能小于1），否则会报错
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="begin_date">获取数据的起始日期，begin_date和end_date的差值需小于“最大时间跨度”（比如最大时间跨度为1时，begin_date和end_date的差值只能为0，才能小于1），否则会报错</param>
        /// <param name="end_date">获取数据的结束日期，end_date允许设置的最大值为昨日</param>
        /// <returns></returns>
        public static string GetUpStreamMsgMonth(string access_token, DateTime begin_date, DateTime end_date)
        {
            var url = string.Format("https://api.weixin.qq.com/datacube/getupstreammsgmonth?access_token={0}", access_token);
            var builder = new StringBuilder();
            builder
                .Append("{")
                .Append('"' + "begin_date" + '"' + ":").Append(begin_date.ToString("yyyy-MM-dd")).Append(",")
                .Append('"' + "end_date" + '"' + ":").Append(end_date.ToString("yyyy-MM-dd"))
                .Append("}");

            var result = NetnrCore.HttpTo.Post(url, builder.ToString());
            return result;
        }

        /// <summary>
        /// 获取消息发送分布数据
        /// 最大时间跨度：15
        /// begin_date和end_date的差值需小于“最大时间跨度”（比如最大时间跨度为1时，begin_date和end_date的差值只能为0，才能小于1），否则会报错
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="begin_date">获取数据的起始日期，begin_date和end_date的差值需小于“最大时间跨度”（比如最大时间跨度为1时，begin_date和end_date的差值只能为0，才能小于1），否则会报错</param>
        /// <param name="end_date">获取数据的结束日期，end_date允许设置的最大值为昨日</param>
        /// <returns></returns>
        public static string GetUpStreamMsgDist(string access_token, DateTime begin_date, DateTime end_date)
        {
            var url = string.Format("https://api.weixin.qq.com/datacube/getupstreammsgdist?access_token={0}", access_token);
            var builder = new StringBuilder();
            builder
                .Append("{")
                .Append('"' + "begin_date" + '"' + ":").Append(begin_date.ToString("yyyy-MM-dd")).Append(",")
                .Append('"' + "end_date" + '"' + ":").Append(end_date.ToString("yyyy-MM-dd"))
                .Append("}");

            var result = NetnrCore.HttpTo.Post(url, builder.ToString());
            return result;
        }

        /// <summary>
        /// 获取消息发送分布周数据
        /// 最大时间跨度：30
        /// begin_date和end_date的差值需小于“最大时间跨度”（比如最大时间跨度为1时，begin_date和end_date的差值只能为0，才能小于1），否则会报错
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="begin_date">获取数据的起始日期，begin_date和end_date的差值需小于“最大时间跨度”（比如最大时间跨度为1时，begin_date和end_date的差值只能为0，才能小于1），否则会报错</param>
        /// <param name="end_date">获取数据的结束日期，end_date允许设置的最大值为昨日</param>
        /// <returns></returns>
        public static string GetUpStreamMsgDistWeek(string access_token, DateTime begin_date, DateTime end_date)
        {
            var url = string.Format("https://api.weixin.qq.com/datacube/getupstreammsgdistweek?access_token={0}", access_token);
            var builder = new StringBuilder();
            builder
                .Append("{")
                .Append('"' + "begin_date" + '"' + ":").Append(begin_date.ToString("yyyy-MM-dd")).Append(",")
                .Append('"' + "end_date" + '"' + ":").Append(end_date.ToString("yyyy-MM-dd"))
                .Append("}");

            var result = NetnrCore.HttpTo.Post(url, builder.ToString());
            return result;
        }

        /// <summary>
        /// 获取消息发送分布月数据
        /// 最大时间跨度：30
        /// begin_date和end_date的差值需小于“最大时间跨度”（比如最大时间跨度为1时，begin_date和end_date的差值只能为0，才能小于1），否则会报错
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="begin_date">获取数据的起始日期，begin_date和end_date的差值需小于“最大时间跨度”（比如最大时间跨度为1时，begin_date和end_date的差值只能为0，才能小于1），否则会报错</param>
        /// <param name="end_date">获取数据的结束日期，end_date允许设置的最大值为昨日</param>
        /// <returns></returns>
        public static string GetUpStreamMsgDistMonth(string access_token, DateTime begin_date, DateTime end_date)
        {
            var url = string.Format("https://api.weixin.qq.com/datacube/getupstreammsgdistmonth?access_token={0}", access_token);
            var builder = new StringBuilder();
            builder
                .Append("{")
                .Append('"' + "begin_date" + '"' + ":").Append(begin_date.ToString("yyyy-MM-dd")).Append(",")
                .Append('"' + "end_date" + '"' + ":").Append(end_date.ToString("yyyy-MM-dd"))
                .Append("}");

            var result = NetnrCore.HttpTo.Post(url, builder.ToString());
            return result;
        }

        /// <summary>
        /// 解释消息类型msg_type
        /// </summary>
        /// <param name="msg_type"></param>
        /// <returns></returns>
        public static string ExplainMsgType(int msg_type)
        {
            switch (msg_type)
            {
                case 1:
                    return "文字";
                case 2:
                    return "图片";
                case 3:
                    return "语音";
                case 4:
                    return "视频";
                case 6:
                    return "第三方应用消息（链接消息）";
                default:
                    return "";
            }
        }
        
        /// <summary>
        /// 解释消息类型msg_type
        /// </summary>
        /// <param name="count_interval"></param>
        /// <returns></returns>
        public static string ExplainCountInterval(int count_interval)
        {
            switch (count_interval)
            {
                case 0:
                    return "0";
                case 1:
                    return "1-5";
                case 2:
                    return "6-10";
                case 3:
                    return "10次以上";
                default:
                    return "";
            }
        }

        /// <summary>
        /// 获取图文群发每日数据
        /// 最大时间跨度：1
        /// begin_date和end_date的差值需小于“最大时间跨度”（比如最大时间跨度为1时，begin_date和end_date的差值只能为0，才能小于1），否则会报错
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="begin_date">获取数据的起始日期，begin_date和end_date的差值需小于“最大时间跨度”（比如最大时间跨度为1时，begin_date和end_date的差值只能为0，才能小于1），否则会报错</param>
        /// <param name="end_date">获取数据的结束日期，end_date允许设置的最大值为昨日</param>
        /// <returns></returns>
        public static string GetArticleSummary(string access_token, DateTime begin_date, DateTime end_date)
        {
            var url = string.Format("https://api.weixin.qq.com/datacube/getarticlesummary?access_token={0}", access_token);
            var builder = new StringBuilder();
            builder
                .Append("{")
                .Append('"' + "begin_date" + '"' + ":").Append(begin_date.ToString("yyyy-MM-dd")).Append(",")
                .Append('"' + "end_date" + '"' + ":").Append(end_date.ToString("yyyy-MM-dd"))
                .Append("}");

            var result = NetnrCore.HttpTo.Post(url, builder.ToString());
            return result;
        }

        /// <summary>
        /// 获取图文群发总数据
        /// 最大时间跨度：1
        /// begin_date和end_date的差值需小于“最大时间跨度”（比如最大时间跨度为1时，begin_date和end_date的差值只能为0，才能小于1），否则会报错
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="begin_date">获取数据的起始日期，begin_date和end_date的差值需小于“最大时间跨度”（比如最大时间跨度为1时，begin_date和end_date的差值只能为0，才能小于1），否则会报错</param>
        /// <param name="end_date">获取数据的结束日期，end_date允许设置的最大值为昨日</param>
        /// <returns></returns>
        public static string GetArticleTotal(string access_token, DateTime begin_date, DateTime end_date)
        {
            var url = string.Format("https://api.weixin.qq.com/datacube/getarticletotal?access_token={0}", access_token);
            var builder = new StringBuilder();
            builder
                .Append("{")
                .Append('"' + "begin_date" + '"' + ":").Append(begin_date.ToString("yyyy-MM-dd")).Append(",")
                .Append('"' + "end_date" + '"' + ":").Append(end_date.ToString("yyyy-MM-dd"))
                .Append("}");

            var result = NetnrCore.HttpTo.Post(url, builder.ToString());
            return result;
        }

        /// <summary>
        /// 获取图文统计数据
        /// 最大时间跨度：3
        /// begin_date和end_date的差值需小于“最大时间跨度”（比如最大时间跨度为1时，begin_date和end_date的差值只能为0，才能小于1），否则会报错
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="begin_date">获取数据的起始日期，begin_date和end_date的差值需小于“最大时间跨度”（比如最大时间跨度为1时，begin_date和end_date的差值只能为0，才能小于1），否则会报错</param>
        /// <param name="end_date">获取数据的结束日期，end_date允许设置的最大值为昨日</param>
        /// <returns></returns>
        public static string GetUserRead(string access_token, DateTime begin_date, DateTime end_date)
        {
            var url = string.Format("https://api.weixin.qq.com/datacube/getuserread?access_token={0}", access_token);
            var builder = new StringBuilder();
            builder
                .Append("{")
                .Append('"' + "begin_date" + '"' + ":").Append(begin_date.ToString("yyyy-MM-dd")).Append(",")
                .Append('"' + "end_date" + '"' + ":").Append(end_date.ToString("yyyy-MM-dd"))
                .Append("}");

            var result = NetnrCore.HttpTo.Post(url, builder.ToString());
            return result;
        }

        /// <summary>
        /// 获取图文统计分时数据
        /// 最大时间跨度：1
        /// begin_date和end_date的差值需小于“最大时间跨度”（比如最大时间跨度为1时，begin_date和end_date的差值只能为0，才能小于1），否则会报错
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="begin_date">获取数据的起始日期，begin_date和end_date的差值需小于“最大时间跨度”（比如最大时间跨度为1时，begin_date和end_date的差值只能为0，才能小于1），否则会报错</param>
        /// <param name="end_date">获取数据的结束日期，end_date允许设置的最大值为昨日</param>
        /// <returns></returns>
        public static string GetUserReadHour(string access_token, DateTime begin_date, DateTime end_date)
        {
            var url = string.Format("https://api.weixin.qq.com/datacube/getuserreadhour?access_token={0}", access_token);
            var builder = new StringBuilder();
            builder
                .Append("{")
                .Append('"' + "begin_date" + '"' + ":").Append(begin_date.ToString("yyyy-MM-dd")).Append(",")
                .Append('"' + "end_date" + '"' + ":").Append(end_date.ToString("yyyy-MM-dd"))
                .Append("}");

            var result = NetnrCore.HttpTo.Post(url, builder.ToString());
            return result;
        }

        /// <summary>
        /// 获取图文分享转发数据
        /// 最大时间跨度：7
        /// begin_date和end_date的差值需小于“最大时间跨度”（比如最大时间跨度为1时，begin_date和end_date的差值只能为0，才能小于1），否则会报错
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="begin_date">获取数据的起始日期，begin_date和end_date的差值需小于“最大时间跨度”（比如最大时间跨度为1时，begin_date和end_date的差值只能为0，才能小于1），否则会报错</param>
        /// <param name="end_date">获取数据的结束日期，end_date允许设置的最大值为昨日</param>
        /// <returns></returns>
        public static string GetUserShare(string access_token, DateTime begin_date, DateTime end_date)
        {
            var url = string.Format("https://api.weixin.qq.com/datacube/getusershare?access_token={0}", access_token);
            var builder = new StringBuilder();
            builder
                .Append("{")
                .Append('"' + "begin_date" + '"' + ":").Append(begin_date.ToString("yyyy-MM-dd")).Append(",")
                .Append('"' + "end_date" + '"' + ":").Append(end_date.ToString("yyyy-MM-dd"))
                .Append("}");

            var result = NetnrCore.HttpTo.Post(url, builder.ToString());
            return result;
        }

        /// <summary>
        /// 获取图文分享转发分时数据
        /// 最大时间跨度：1
        /// begin_date和end_date的差值需小于“最大时间跨度”（比如最大时间跨度为1时，begin_date和end_date的差值只能为0，才能小于1），否则会报错
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="begin_date">获取数据的起始日期，begin_date和end_date的差值需小于“最大时间跨度”（比如最大时间跨度为1时，begin_date和end_date的差值只能为0，才能小于1），否则会报错</param>
        /// <param name="end_date">获取数据的结束日期，end_date允许设置的最大值为昨日</param>
        /// <returns></returns>
        public static string GetUserShareHour(string access_token, DateTime begin_date, DateTime end_date)
        {
            var url = string.Format("https://api.weixin.qq.com/datacube/getusersharehour?access_token={0}", access_token);
            var builder = new StringBuilder();
            builder
                .Append("{")
                .Append('"' + "begin_date" + '"' + ":").Append(begin_date.ToString("yyyy-MM-dd")).Append(",")
                .Append('"' + "end_date" + '"' + ":").Append(end_date.ToString("yyyy-MM-dd"))
                .Append("}");

            var result = NetnrCore.HttpTo.Post(url, builder.ToString());
            return result;
        }

        /// <summary>
        /// 解释用户分享的场景share_scene
        /// </summary>
        /// <param name="share_scene"></param>
        /// <returns></returns>
        public static string ExplainShareScene(int share_scene)
        {
            switch (share_scene)
            {
                case 1:
                    return "好友转发";
                case 2:
                    return "朋友圈";
                case 3:
                    return "腾讯微博";
                case 255:
                    return "其他";
                default:
                    return "";
            }
        }

        /// <summary>
        /// 获取用户增减数据
        /// 最大时间跨度：7
        /// begin_date和end_date的差值需小于“最大时间跨度”（比如最大时间跨度为1时，begin_date和end_date的差值只能为0，才能小于1），否则会报错
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="begin_date">获取数据的起始日期，begin_date和end_date的差值需小于“最大时间跨度”（比如最大时间跨度为1时，begin_date和end_date的差值只能为0，才能小于1），否则会报错</param>
        /// <param name="end_date">获取数据的结束日期，end_date允许设置的最大值为昨日</param>
        /// <returns></returns>
        public static string GetUserSummary(string access_token, DateTime begin_date, DateTime end_date)
        {
            var url = string.Format("https://api.weixin.qq.com/datacube/getusersummary?access_token={0}", access_token);
            var builder = new StringBuilder();
            builder
                .Append("{")
                .Append('"' + "begin_date" + '"' + ":").Append(begin_date.ToString("yyyy-MM-dd")).Append(",")
                .Append('"' + "end_date" + '"' + ":").Append(end_date.ToString("yyyy-MM-dd"))
                .Append("}");

            var result = NetnrCore.HttpTo.Post(url, builder.ToString());
            return result;
        }

        /// <summary>
        /// 解释用户的渠道user_source
        /// </summary>
        /// <param name="user_source"></param>
        /// <returns></returns>
        public static string ExplainUserSource(int user_source)
        {
            switch (user_source)
            {
                case 0:
                    return "其他";
                case 30:
                    return "扫二维码";
                case 17:
                    return "名片分享";
                case 35:
                    return "搜号码（微信添加朋友页的搜索）";
                case 39:
                    return "查询微信公众帐号 ";
                case 43:
                    return "图文页右上角菜单";
                default:
                    return "";
            }
        }
    }
}
