using System.Text;

namespace Netnr.WeChat
{
    /// <summary>
    /// 
    /// </summary>
    public class Semantic
    {
        /// <summary>
        /// 
        /// </summary>
        public class Semproxy
        {
            /// <summary>
            /// 语义理解
            /// </summary>
            /// <param name="access_token">根据appid和appsecret获取到的token</param>
            /// <param name="query">输入文本串</param>
            /// <param name="category">需要使用的服务类型，多个用“，”隔开，不能为空</param>
            /// <param name="latitude">纬度坐标，与经度同时传入；与城市二选一传入</param>
            /// <param name="longitude">经度坐标，与纬度同时传入；与城市二选一传入</param>
            /// <param name="city">城市名称，与经纬度二选一传入</param>
            /// <param name="region">区域名称，在城市存在的情况下可省；与经纬度二选一传入</param>
            /// <param name="appid">公众号唯一标识，用于区分公众号开发者</param>
            /// <param name="uid">用户唯一id（非开发者id），用户区分公众号下的不同用户（建议填入用户openid），如果为空，则无法使用上下文理解功能。appid和uid同时存在的情况下，才可以使用上下文理解功能。</param>
            /// <returns></returns>
            public static string Semantic(string access_token, string query, string category, string latitude, string longitude, string city, string region, string appid, string uid)
            {
                var builder = new StringBuilder();
                builder
                    .Append("{")
                    .Append('"' + "query" + '"' + ":").Append(query).Append(",")
                    .Append('"' + "category" + '"' + ":").Append(category).Append(",")
                    .Append('"' + "latitude" + '"' + ":").Append(latitude).Append(",")
                    .Append('"' + "longitude" + '"' + ":").Append(longitude).Append(",")
                    .Append('"' + "city" + '"' + ":").Append(city).Append(",")
                    .Append('"' + "region" + '"' + ":").Append(region).Append(",")
                    .Append('"' + "appid" + '"' + ":").Append(appid).Append(",")
                    .Append('"' + "uid" + '"' + ":").Append(uid).Append(",")
                    .Append("}");

                var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/semantic/semproxy/search?access_token={0}", access_token), builder.ToString());
                return result;
            }
        }
    }
}
