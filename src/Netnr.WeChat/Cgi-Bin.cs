using System.IO;
using System.Text;
using Netnr.WeChat.Entities;
using Netnr.WeChat.Helpers;
using System.Collections.Generic;

namespace Netnr.WeChat
{
    /// <summary>
    /// 
    /// </summary>
    public class Cgi_Bin
    {
        /// <summary>
        /// 获取AccessToken
        /// http://mp.weixin.qq.com/wiki/index.php?title=%E8%8E%B7%E5%8F%96access_token
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="secrect"></param>
        /// <returns>access_toke</returns>
        public static string Token(string appid, string secrect)
        {
            var url = string.Format("https://api.weixin.qq.com/cgi-bin/token?grant_type={0}&appid={1}&secret={2}", "client_credential", appid, secrect);
            var result = NetnrCore.HttpTo.Get(url);
            return result;
        }

        /// <summary>
        /// 获取微信服务器IP地址
        ///http://mp.weixin.qq.com/wiki/0/2ad4b6bfd29f30f71d39616c2a0fcedc.html
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns>{"ip_list":["127.0.0.1","127.0.0.1"]}</returns>
        public static string GetCallbackIP(string access_token)
        {
            var url = string.Format("https://api.weixin.qq.com/cgi-bin/getcallbackip?access_token={0}", access_token);
            var result = NetnrCore.HttpTo.Get(url);
            return result;
        }

        /// <summary>
        ///上传LOGO接口
        ///开发者需调用该接口上传商户图标至微信服务器，获取相应logo_url，用于卡券创建。
        ///注意事项
        ///1.上传的图片限制文件大小限制1MB，像素为300*300，支持JPG格式。
        ///2.调用接口获取的logo_url进支持在微信相关业务下使用，否则会做相应处理。
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="fileName"></param>
        /// <param name="inputStream">流</param>
        /// <returns>返回上传后路径</returns>
        public static string UploadImg(string access_token, string fileName, Stream inputStream)
        {
            var url = string.Format("http://api.weixin.qq.com/cgi-bin/uploadimg?access_token={0}", access_token);
            var returnMessage = Util.HttpRequestPost(url, "buffer", fileName, inputStream);
            return returnMessage;
        }

        /// <summary>
        /// 将一条长链接转成短链接。
        /// 主要使用场景： 开发者用于生成二维码的原链接（商品、支付二维码等）太长导致扫码速度和成功率下降，
        /// 将原长链接通过此接口转成短链接再生成二维码将大大提升扫码速度和成功率。
        /// </summary>
        /// <param name="access_token">调用接口凭证</param>
        /// <param name="long_url">需要转换的长链接，支持http://、https://、weixin://wxpay 格式的url</param>
        /// <param name="action">默认long2short，代表长链接转短链接</param>
        /// <returns></returns>
        public static string ShortUrl(string access_token, string long_url, string action = "long2short")
        {
            var builder = new StringBuilder();
            builder
                .Append("{")
                .Append('"' + "action" + '"' + ":").Append(action).Append(",")
                .Append('"' + "long_url" + '"' + ":").Append(long_url)
                .Append("}");

            var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/cgi-bin/shorturl?access_token={0}", access_token), builder.ToString());
            return result;
        }

        /// <summary>
        /// 得到二维码的微信服务器地址
        /// </summary>
        /// <param name="ticket">票</param>
        /// <returns></returns>
        public static string ShowQrCode(string ticket)
        {
            return string.Format("https://mp.weixin.qq.com/cgi-bin/showqrcode?ticket={0}", ticket);
        }

        /// <summary>
        /// 
        /// </summary>
        public class Media
        {
            /// <summary>
            /// 上传图文消息素材【订阅号与服务号认证后均可用】
            /// thumb_media_id:图文消息缩略图的media_id，可以在基础支持-上传多媒体文件接口中获得
            /// </summary>
            /// <param name="access_token"></param>
            /// <param name="articles">图文消息，一个图文消息支持1到10条图文</param>
            /// <returns>success: { "type":"news","media_id":"CsEf3ldqkAYJAU6EJeIkStVDSvffUJ54vqbThMgplD-VJXXof6ctX5fI6-aYyUiQ", "created_at":1391857799}</returns>
            public static string UploadArtcles(string access_token, List<WeChatArtcle> articles)
            {
                var url = string.Format("https://api.weixin.qq.com/cgi-bin/media/uploadnews?access_token={0}", access_token);
                var result = NetnrCore.HttpTo.Post(url, NetnrCore.ToJson(articles));
                return result;
            }

            /// <summary>
            /// 回复复视频消息里面的media_id不是基础支持接口返回的media_id，这里需要给基础支持的media添加title和description
            /// </summary>
            /// <param name="access_token"></param>
            /// <param name="media_id"></param>
            /// <param name="title"></param>
            /// <param name="description"></param>
            /// <returns>success:{"type":"video","media_id":"IhdaAQXuvJtGzwwc0abfXnzeezfO0NgPK6AQYShD8RQYMTtfzbLdBIQkQziv2XJc","created_at":1398848981}
            /// </returns>
            public static string UploadVideo(string access_token, string media_id, string title, string description)
            {
                var url = string.Format("https://file.api.weixin.qq.com/cgi-bin/media/uploadvideo?access_token={0}", access_token);
                var builder = new StringBuilder();
                builder
                    .Append("{")
                    .Append('"' + "media_id" + '"' + ":").Append(media_id).Append(",")
                    .Append('"' + "title" + '"' + ":").Append(title)
                    .Append('"' + "description" + '"' + ":").Append(description)
                    .Append("}");
                var result = NetnrCore.HttpTo.Post(url, builder.ToString());
                return result;
            }

            /// <summary>
            /// 新增临时素材/上传多媒体文件
            /// http://mp.weixin.qq.com/wiki/5/963fc70b80dc75483a271298a76a8d59.html
            /// 1.上传的媒体文件限制：
            ///图片（image) : 1MB，支持JPG格式
            ///语音（voice）：1MB，播放长度不超过60s，支持MP4格式
            ///视频（video）：10MB，支持MP4格式
            ///缩略图（thumb)：64KB，支持JPG格式
            ///2.媒体文件在后台保存时间为3天，即3天后media_id失效
            /// </summary>
            /// <param name="access_token"></param>
            /// <param name="type">媒体文件类型，分别有图片（image）、语音（voice）、视频（video）和缩略图（thumb）</param>
            /// <param name="fileName">文件名</param>
            /// <param name="inputStream">文件输入流</param>
            /// <returns>media_id</returns>
            public static string Upload(string access_token, string type, string fileName, Stream inputStream)
            {
                var url = string.Format("http://file.api.weixin.qq.com/cgi-bin/media/upload?access_token={0}&type={1}", access_token, type.ToString());
                var result = Util.HttpRequestPost(url, "media", fileName, inputStream);
                inputStream.Close();
                inputStream.Dispose();
                return result;
            }

            /// <summary>
            /// 获取临时素材/下载多媒体文件
            /// http://mp.weixin.qq.com/wiki/11/07b6b76a6b6e8848e855a435d5e34a5f.html
            /// 公众号可以使用本接口获取临时素材（即下载临时的多媒体文件）。请注意，视频文件不支持https下载，调用该接口需http协议。
            /// 本接口即为原“下载多媒体文件”接口。
            /// </summary>
            /// <param name="savePath"></param>
            /// <param name="access_token"></param>
            /// <param name="media_id"></param>
            public static void Get(string savePath, string access_token, string media_id)
            {
                var url = string.Format("http://file.api.weixin.qq.com/cgi-bin/media/get?access_token={0}&media_id={1}", access_token, media_id);
                FileStream fs = new(savePath, FileMode.Create);
                Util.Download(url, fs);
                fs.Close();
                fs.Dispose();
            }

            /// <summary>
            /// 添加客服账号
            /// 开发者通过本接口可以为公众号添加客服账号，每个公众号最多添加10个客服账号。
            /// </summary>
            /// <param name="access_token"></param>
            /// <param name="kf_account">完整客服账号，格式为：账号前缀@公众号微信号，账号前缀最多10个字符，必须是英文或者数字字符。如果没有公众号微信号，请前往微信公众平台设置。</param>
            /// <param name="nickname">客服昵称，最长6个汉字或12个英文字符</param>
            /// <param name="pswmd5">客服账号登录密码，格式为密码明文的32位加密MD5值</param>
            /// <returns>success: {"errcode" : 0,"errmsg" : "ok",}； 其他代码请使用ExplainCode解析返回代码含义 </returns>
            public static string UploadNews(string access_token, string kf_account, string nickname, string pswmd5)
            {
                var url = string.Format("https://api.weixin.qq.com/cgi-bin/media/uploadnews?access_token={0}", access_token);
                var builder = new StringBuilder();
                builder
                    .Append("{")
                    .Append('"' + "kf_account" + '"' + ":").Append(kf_account).Append(",")
                    .Append('"' + "nickname" + '"' + ":").Append(nickname).Append(",")
                    .Append('"' + "password" + '"' + ":").Append(pswmd5)
                    .Append("}");

                var result = NetnrCore.HttpTo.Post(url, builder.ToString());
                return result;
            }
        }

        /// <summary>
        /// "自定义菜单”
        /// http://mp.weixin.qq.com/wiki/index.php?title=%E8%87%AA%E5%AE%9A%E4%B9%89%E8%8F%9C%E5%8D%95%E5%88%9B%E5%BB%BA%E6%8E%A5%E5%8F%A3
        /// 注意：自定义菜单事件推送接口见：AcceptMessageAPI
        /// 创建自定义菜单后，由于微信客户端缓存，需要24小时微信客户端才会展现出来，测试时可以尝试取消关注公众账号后再次关注，则可以看到创建后的效果。
        /// 自定义菜单种类如下：
        /// 1、click：点击推事件
        /// 2、view：跳转URL
        /// 3、scancode_push：扫码推事件
        /// 4、scancode_waitmsg：扫码推事件且弹出“消息接收中”提示框
        /// 5、pic_sysphoto：弹出系统拍照发图
        /// 6、pic_photo_or_album：弹出拍照或者相册发图
        /// 7、pic_weixin：弹出微信相册发图器
        /// 8、location_select：弹出地理位置选择器
        /// </summary>
        public class Menu
        {
            /// <summary>
            /// 自定义菜单创建接口
            /// </summary>
            /// <param name="token"></param>
            /// <param name="content"></param>
            /// <returns></returns>
            public static string Create(string token, string content)
            {
                var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/cgi-bin/menu/create?access_token={0}", token), content);
                return result;
            }

            /// <summary>
            /// 自定义菜单查询接口
            /// </summary>
            /// <param name="token"></param>
            /// <returns></returns>
            public static string Get(string token)
            {
                var result = NetnrCore.HttpTo.Get(string.Format("https://api.weixin.qq.com/cgi-bin/menu/get?access_token={0}", token));
                return result;
            }

            /// <summary>
            /// 自定义菜单删除接口
            /// </summary>
            /// <param name="token"></param>
            /// <returns></returns>
            public static string Delete(string token)
            {
                var result = NetnrCore.HttpTo.Get(string.Format("https://api.weixin.qq.com/cgi-bin/menu/delete?access_token={0}", token));
                return result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class Template
        {
            /// <summary>
            /// 设置所属行业
            /// 设置行业可在MP中完成，每月可修改行业1次
            /// 行业代码查询,请登录微信公众号后台查看
            /// </summary>
            /// <param name="access_token"></param>
            /// <param name="industry_id1">行业代码</param>
            /// <param name="industry_id2">行业代码</param>
            /// <returns>官方api未给出返回内容,应该是errcode=0就表示成功</returns>
            public static string Api_Set_Industry(string access_token, string industry_id1, string industry_id2)
            {
                var url = string.Format("https://api.weixin.qq.com/cgi-bin/template/api_set_industry?access_token={0}", access_token);

                var builder = new StringBuilder();
                builder
                    .Append("{")
                    .Append('"' + "industry_id1" + '"' + ":").Append(industry_id1).Append(",")
                    .Append('"' + "industry_id2" + '"' + ":").Append(industry_id2)
                    .Append("}");
                var result = NetnrCore.HttpTo.Post(url, builder.ToString());
                return result;
            }
            /// <summary>
            /// 获得模板ID
            /// </summary>
            /// <param name="access_token"></param>
            /// <param name="template_id_short">模板库中模板的编号</param>
            /// <returns> {"errcode":0,"errmsg":"ok","template_id":"Doclyl5uP7Aciu-qZ7mJNPtWkbkYnWBWVja26EGbNyk"}
            /// </returns>
            public static string Api_Add_Template(string access_token, string template_id_short)
            {
                var url = string.Format("https://api.weixin.qq.com/cgi-bin/template/api_add_template?access_token={0}", access_token);

                var builder = new StringBuilder();
                builder
                    .Append("{")
                    .Append('"' + "template_id_short" + '"' + ":").Append(template_id_short)
                    .Append("}");
                var result = NetnrCore.HttpTo.Post(url, builder.ToString());
                return result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class Ticket
        {
            /// <summary>
            /// 获取api_ticket
            /// 正常情况下，jsapi_ticket的有效期为7200秒，通过access_token来获取。
            /// </summary>
            /// <param name="access_token">BasicAPI获取的access_token,也可以通过TokenHelper获取</param>
            /// <param name="type">类型：wx_card、jsapi</param>
            /// <returns></returns>
            public static string GetTicket(string access_token, string type)
            {
                var url = string.Format("https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token={0}&type={1}", access_token, type);
                var result = NetnrCore.HttpTo.Get(url);
                return result;
            }

            /// <summary>
            /// 签名算法
            /// </summary>
            /// <param name="jsapi_ticket">jsapi_ticket</param>
            /// <param name="noncestr">随机字符串(必须与wx.config中的nonceStr相同)</param>
            /// <param name="timestamp">时间戳(必须与wx.config中的timestamp相同)</param>
            /// <param name="url">当前网页的URL，不包含#及其后面部分(必须是调用JS接口页面的完整URL)</param>
            /// <param name="string1"></param>
            /// <returns></returns>
            public static string GetSignature(string jsapi_ticket, string noncestr, long timestamp, string url, out string string1)
            {
                var string1Builder = new StringBuilder();
                string1Builder.Append("jsapi_ticket=").Append(jsapi_ticket).Append("&")
                              .Append("noncestr=").Append(noncestr).Append("&")
                              .Append("timestamp=").Append(timestamp).Append("&")
                              .Append("url=").Append(url.IndexOf("#") >= 0 ? url.Substring(0, url.IndexOf("#")) : url);
                string1 = string1Builder.ToString();
                return Util.Sha1(string1);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class Groups
        {
            /// <summary>
            /// 创建分组
            /// 注意：一个公众账号，最多支持创建500个分组
            /// </summary>
            /// <param name="access_token">调用接口凭证</param>
            /// <param name="name">分组名字（30个字符以内）</param>
            /// <returns></returns>
            public static string Create(string access_token, string name)
            {
                var builder = new StringBuilder();
                builder.Append("{")
                    .Append('"' + "group" + '"' + ":")
                    .Append("{")
                    .Append('"' + "name" + '"' + ":").Append(name)
                    .Append("}")
                    .Append("}");

                var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/cgi-bin/groups/create?access_token={0}", access_token), builder.ToString());
                return result;
            }

            /// <summary>
            /// 查询所有分组
            /// </summary>
            /// <param name="access_token">调用接口凭证</param>
            /// <returns></returns>
            public static string Get(string access_token)
            {
                var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/cgi-bin/groups/get?access_token={0}", access_token), "");
                return result;
            }

            /// <summary>
            /// 查询用户所在分组
            /// </summary>
            /// <param name="access_token"></param>
            /// <param name="openid"></param>
            /// <returns>不为空时，表示查询成功，否则查询失败</returns>
            public static string GetId(string access_token, string openid)
            {
                var builder = new StringBuilder();
                builder.Append("{").Append('"' + "openid" + '"' + ":").Append(openid).Append("}");
                var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/cgi-bin/groups/getid?access_token={0}", access_token), builder.ToString());
                return result;
            }

            /// <summary>
            /// 修改分组名
            /// </summary>
            /// <param name="access_token">调用接口凭证</param>
            /// <param name="id">分组id，由微信分配</param>
            /// <param name="name">分组名字（30个字符以内）</param>
            /// <returns></returns>
            public static string Update(string access_token, string id, string name)
            {
                var builder = new StringBuilder();
                builder.Append("{")
                    .Append('"' + "group" + '"' + ":")
                    .Append("{")
                    .Append('"' + "id" + '"' + ":").Append(id).Append(",")
                    .Append('"' + "name" + '"' + ":").Append(name)
                    .Append("}")
                    .Append("}");

                var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/cgi-bin/groups/update?access_token={0}", access_token), builder.ToString());
                return result;
            }

            /// <summary>
            /// 
            /// </summary>
            public class Members
            {
                /// <summary>
                /// 移动用户分组
                /// </summary>
                /// <param name="access_token">调用接口凭证</param>
                /// <param name="openid">用户唯一标识符</param>
                /// <param name="to_groupid">分组id</param>
                /// <returns></returns>
                public static string Update(string access_token, string openid, string to_groupid)
                {
                    var builder = new StringBuilder();
                    builder.Append("{")
                        .Append('"' + "openid" + '"' + ":").Append(openid).Append(",")
                        .Append('"' + "to_groupid" + '"' + ":").Append(to_groupid)
                        .Append("}");

                    var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/cgi-bin/groups/members/update?access_token={0}", access_token), builder.ToString());
                    return result;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class Message
        {
            /// <summary>
            /// 发送(主动)客服消息
            /// </summary>
            public class Custom
            {
                /// <summary>
                /// 发送文本消息
                /// </summary>
                /// <param name="access_token">调用接口凭证</param>
                /// <param name="touser">普通用户openid</param>
                /// <param name="content">文本消息内容</param>
                /// <param name="kf_account">完整客服账号，格式为：账号前缀@公众号微信号</param>
                public static void SendText(string access_token, string touser, string content, string kf_account = null)
                {
                    var builder = new StringBuilder();
                    builder.Append("{")
                        .Append('"' + "touser" + '"' + ":").Append('"' + touser + '"').Append(",")
                        .Append('"' + "msgtype" + '"' + ":").Append('"' + "text" + '"').Append(",")
                        .Append('"' + "text" + '"' + ":")
                        .Append("{")
                        .Append('"' + "content" + '"' + ":").Append('"' + content + '"')
                        .Append("}");
                    if (!string.IsNullOrEmpty(kf_account))
                    {
                        builder.Append(",");
                        builder.Append('"' + "customservice" + '"' + ":")
                               .Append("{")
                               .Append('"' + "kfaccount" + '"' + ":").Append('"' + kf_account + '"')
                               .Append("}");
                    }
                    builder.Append("}");

                    NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/cgi-bin/message/custom/send?access_token={0}", access_token), builder.ToString());
                }

                /// <summary>
                /// 发送图片消息
                /// </summary>
                /// <param name="access_token">调用接口凭证</param>
                /// <param name="touser">普通用户openid</param>
                /// <param name="media_id">发送的图片的媒体ID</param>
                /// <param name="kf_account">完整客服账号，格式为：账号前缀@公众号微信号</param>
                /// <returns></returns>
                public static void SendImage(string access_token, string touser, string media_id, string kf_account = null)
                {
                    var builder = new StringBuilder();
                    builder.Append("{")
                        .Append('"' + "touser" + '"' + ":").Append('"' + touser + '"').Append(",")
                        .Append('"' + "msgtype" + '"' + ":").Append('"' + "image" + '"').Append(",")
                        .Append('"' + "image" + '"' + ":")
                        .Append("{")
                        .Append('"' + "media_id" + '"' + ":").Append('"' + media_id + '"')
                        .Append("}");
                    if (!string.IsNullOrEmpty(kf_account))
                    {
                        builder.Append(",");
                        builder.Append('"' + "customservice" + '"' + ":")
                               .Append("{")
                               .Append('"' + "kfaccount" + '"' + ":").Append('"' + kf_account + '"')
                               .Append("}");
                    }
                    builder.Append("}");

                    NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/cgi-bin/message/custom/send?access_token={0}", access_token), builder.ToString());
                }

                /// <summary>
                /// 发送语音消息
                /// </summary>
                /// <param name="access_token">调用接口凭证</param>
                /// <param name="touser">普通用户openid</param>
                /// <param name="media_id">发送的语音的媒体ID</param>
                /// <param name="kf_account">完整客服账号，格式为：账号前缀@公众号微信号</param>
                /// <returns></returns>
                public static void SendVoice(string access_token, string touser, string media_id, string kf_account = null)
                {
                    var builder = new StringBuilder();
                    builder.Append("{")
                        .Append('"' + "touser" + '"' + ":").Append('"' + touser + '"').Append(",")
                        .Append('"' + "msgtype" + '"' + ":").Append('"' + "voice" + '"').Append(",")
                        .Append('"' + "voice" + '"' + ":")
                        .Append("{")
                        .Append('"' + "media_id" + '"' + ":").Append('"' + media_id + '"')
                        .Append("}");
                    if (!string.IsNullOrEmpty(kf_account))
                    {
                        builder.Append(",");
                        builder.Append('"' + "customservice" + '"' + ":")
                               .Append("{")
                               .Append('"' + "kfaccount" + '"' + ":").Append('"' + kf_account + '"')
                               .Append("}");
                    }
                    builder.Append("}");

                    NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/cgi-bin/message/custom/send?access_token={0}", access_token), builder.ToString());
                }

                /// <summary>
                /// 发送视频消息
                /// </summary>
                /// <param name="access_token">调用接口凭证</param>
                /// <param name="touser">普通用户openid</param>
                /// <param name="media_id">发送的视频的媒体ID</param>
                /// <param name="thumb_media_id">缩略图的媒体ID</param>
                /// <param name="title">视频消息的标题</param>
                /// <param name="description">视频消息的描述</param>
                /// <param name="kf_account">完整客服账号，格式为：账号前缀@公众号微信号</param>
                /// <returns></returns>
                public static void SendVedio(string access_token, string touser, string media_id, string thumb_media_id, string title, string description, string kf_account = null)
                {
                    var builder = new StringBuilder();
                    builder.Append("{")
                        .Append('"' + "touser" + '"' + ":").Append('"' + touser + '"').Append(",")
                        .Append('"' + "msgtype" + '"' + ":").Append('"' + "video" + '"').Append(",")
                        .Append('"' + "video" + '"' + ":")
                        .Append("{")
                        .Append('"' + "media_id" + '"' + ":").Append('"' + media_id + '"').Append(",")
                        .Append('"' + "thumb_media_id" + '"' + ":").Append('"' + thumb_media_id + '"').Append(",")
                        .Append('"' + "title" + '"' + ":").Append('"' + title + '"').Append(",")
                        .Append('"' + "description" + '"' + ":").Append('"' + description + '"')
                        .Append("}");
                    if (!string.IsNullOrEmpty(kf_account))
                    {
                        builder.Append(",");
                        builder.Append('"' + "customservice" + '"' + ":")
                               .Append("{")
                               .Append('"' + "kfaccount" + '"' + ":").Append('"' + kf_account + '"')
                               .Append("}");
                    }
                    builder.Append("}");

                    NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/cgi-bin/message/custom/send?access_token={0}", access_token), builder.ToString());
                }

                /// <summary>
                /// 发送音乐消息
                /// </summary>
                /// <param name="access_token">调用接口凭证</param>
                /// <param name="touser">普通用户openid</param>
                /// <param name="musicurl">音乐链接</param>
                /// <param name="hqmusicurl">高品质音乐链接，wifi环境优先使用该链接播放音乐</param>
                /// <param name="thumb_media_id">缩略图的媒体ID</param>
                /// <param name="title">音乐标题</param>
                /// <param name="description">音乐描述</param>
                /// <param name="kf_account">完整客服账号，格式为：账号前缀@公众号微信号</param>
                /// <returns></returns>
                public static void SendMusic(string access_token, string touser, string musicurl, string hqmusicurl, string thumb_media_id, string title, string description, string kf_account = null)
                {
                    var builder = new StringBuilder();
                    builder.Append("{")
                        .Append('"' + "touser" + '"' + ":").Append('"' + touser + '"').Append(",")
                        .Append('"' + "msgtype" + '"' + ":").Append('"' + "music" + '"').Append(",")
                        .Append('"' + "music" + '"' + ":")
                        .Append("{")
                        .Append('"' + "title" + '"' + ":").Append('"' + title + '"').Append(",")
                        .Append('"' + "description" + '"' + ":").Append('"' + description + '"').Append(",")
                        .Append('"' + "musicurl" + '"' + ":").Append('"' + musicurl + '"').Append(",")
                        .Append('"' + "hqmusicurl" + '"' + ":").Append('"' + hqmusicurl + '"').Append(",")
                        .Append('"' + "thumb_media_id" + '"' + ":").Append('"' + thumb_media_id + '"').Append(",")
                        .Append("}");
                    if (!string.IsNullOrEmpty(kf_account))
                    {
                        builder.Append(",");
                        builder.Append('"' + "customservice" + '"' + ":")
                               .Append("{")
                               .Append('"' + "kfaccount" + '"' + ":").Append('"' + kf_account + '"')
                               .Append("}");
                    }
                    builder.Append("}");

                    NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/cgi-bin/message/custom/send?access_token={0}", access_token), builder.ToString());
                }

                /// <summary>
                /// 回复单图文消息
                /// </summary>
                /// <param name="access_token">调用接口凭证</param>
                /// <param name="touser">普通用户openid</param>
                /// <param name="news"></param>
                /// <param name="kf_account">完整客服账号，格式为：账号前缀@公众号微信号</param>
                /// <returns></returns>
                public static void SendNews(string access_token, string touser, WeChatNews news, string kf_account = null)
                {
                    var builder = new StringBuilder();
                    builder.Append("{")
                        .Append('"' + "touser" + '"' + ":").Append('"' + touser + '"').Append(",")
                        .Append('"' + "msgtype" + '"' + ":").Append('"' + "news" + '"').Append(",")
                        .Append('"' + "news" + '"' + ":")
                        .Append("{")
                           .Append('"' + "articles" + '"' + ":")
                             .Append("[")
                             .Append("{")
                             .Append('"' + "title" + '"' + ":").Append('"' + news.title + '"').Append(",")
                             .Append('"' + "description" + '"' + ":").Append('"' + news.description + '"').Append(",")
                             .Append('"' + "url" + '"' + ":").Append('"' + news.url + '"').Append(",")
                             .Append('"' + "picurl" + '"' + ":").Append('"' + news.picurl + '"')
                             .Append("}")
                           .Append("]")
                        .Append("}");
                    if (!string.IsNullOrEmpty(kf_account))
                    {
                        builder.Append(",");
                        builder.Append('"' + "customservice" + '"' + ":")
                               .Append("{")
                               .Append('"' + "kfaccount" + '"' + ":").Append('"' + kf_account + '"')
                               .Append("}");
                    }
                    builder.Append("}");

                    NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/cgi-bin/message/custom/send?access_token={0}", access_token), builder.ToString());
                }

                /// <summary>
                /// 回复多图文消息
                /// </summary>
                /// <param name="access_token">调用接口凭证</param>
                /// <param name="touser">普通用户openid</param>
                /// <param name="news"></param>
                /// <param name="kf_account">完整客服账号，格式为：账号前缀@公众号微信号</param>
                /// <returns></returns>
                public static void SendNews(string access_token, string touser, List<WeChatNews> news, string kf_account = null)
                {
                    var builder = new StringBuilder();
                    builder.Append("{")
                        .Append('"' + "touser" + '"' + ":").Append('"' + touser + '"').Append(",")
                        .Append('"' + "msgtype" + '"' + ":").Append('"' + "news" + '"').Append(",")
                        .Append('"' + "news" + '"' + ":")
                        .Append("{").Append('"' + "articles" + '"' + ":").Append("[");
                    for (int i = 0; i < news.Count; i++)
                    {
                        var n = news[i];
                        builder.Append("{")
                               .Append('"' + "title" + '"' + ":").Append('"' + n.title + '"').Append(",")
                               .Append('"' + "description" + '"' + ":").Append('"' + n.description + '"').Append(",")
                               .Append('"' + "url" + '"' + ":").Append('"' + n.url + '"').Append(",")
                               .Append('"' + "picurl" + '"' + ":").Append('"' + n.picurl + '"')
                               .Append("}");
                        if (i != news.Count - 1) builder.Append(",");
                    }
                    builder.Append("]")
                           .Append("}");
                    if (!string.IsNullOrEmpty(kf_account))
                    {
                        builder.Append(",");
                        builder.Append('"' + "customservice" + '"' + ":")
                               .Append("{")
                               .Append('"' + "kfaccount" + '"' + ":").Append('"' + kf_account + '"')
                               .Append("}");
                    }
                    builder.Append("}");

                    NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/cgi-bin/message/custom/send?access_token={0}", access_token), builder.ToString());
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public class Mass
            {
                /// <summary>
                /// 根据分组进行群发【订阅号与服务号认证后均可用】
                /// 请注意：在返回成功时，意味着群发任务提交成功，并不意味着此时群发已经结束，
                /// 所以，仍有可能在后续的发送过程中出现异常情况导致用户未收到消息，如消息有时会进行审核、服务器不稳定等。
                /// 此外，群发任务一般需要较长的时间才能全部发送完毕，请耐心等待。
                /// </summary>
                /// <param name="access_token"></param>
                /// <param name="content">图文消息,语音,图片,视频:media_id; 文本:文本消息</param>
                /// <param name="type"></param>
                /// <param name="group_id">群发到的分组的group_id，参加用户管理中用户分组接口，若is_to_all值为true，可不填写group_id</param>
                /// <param name="is_to_all">用于设定是否向全部用户发送，值为true或false，选择true该消息群发给所有用户，选择false可根据group_id发送给指定群组的用户</param>
                /// <returns>success:{"errcode":0,"errmsg":"send job submission success","msg_id":34182 }</returns>
                public static string SendAll(string access_token, string content, WeChatArtcleType type, string group_id, bool is_to_all = false)
                {
                    var url = string.Format("https://api.weixin.qq.com/cgi-bin/message/mass/sendall?access_token={0}", access_token);
                    var builder = new StringBuilder();
                    builder.Append("{")
                           .Append('"' + "filter" + '"' + ":")
                                                   .Append("{")
                                                   .Append('"' + "is_to_all" + '"' + ":").Append(is_to_all).Append(",")
                                                   .Append('"' + "group_id" + '"' + ":").Append(group_id)
                                                   .Append("},");

                    switch (type)
                    {
                        case WeChatArtcleType.News:
                            builder.Append('"' + "mpnews" + '"' + ":")
                                                   .Append("{")
                                                   .Append('"' + "media_id" + '"' + ":").Append(content)
                                                   .Append("},")
                                      .Append('"' + "msgtype" + '"' + ":").Append("mpnews");
                            break;
                        case WeChatArtcleType.Text:
                            builder.Append('"' + "text" + '"' + ":")
                                                   .Append("{")
                                                   .Append('"' + "content" + '"' + ":").Append(content)
                                                   .Append("},")
                                      .Append('"' + "msgtype" + '"' + ":").Append("text");
                            break;
                        case WeChatArtcleType.Voice:
                            builder.Append('"' + "voice" + '"' + ":")
                                                   .Append("{")
                                                   .Append('"' + "media_id" + '"' + ":").Append(content)
                                                   .Append("},")
                                      .Append('"' + "msgtype" + '"' + ":").Append("voice");
                            break;
                        case WeChatArtcleType.Image:
                            builder.Append('"' + "image" + '"' + ":")
                                                   .Append("{")
                                                   .Append('"' + "media_id" + '"' + ":").Append(content)
                                                   .Append("},")
                                      .Append('"' + "msgtype" + '"' + ":").Append("image");
                            break;
                        case WeChatArtcleType.Video:
                            builder.Append('"' + "mpvideo" + '"' + ":")
                                                   .Append("{")
                                                   .Append('"' + "media_id" + '"' + ":").Append(content)
                                                   .Append("},")
                                      .Append('"' + "msgtype" + '"' + ":").Append("mpvideo");
                            break;
                    }
                    builder.Append("}");
                    var result = NetnrCore.HttpTo.Post(url, builder.ToString());
                    return result;
                }

                /// <summary>
                /// 根据OpenID列表群发【订阅号不可用，服务号认证后可用】
                /// </summary>
                /// <param name="access_token"></param>
                /// <param name="content">图文消息,语音,图片,视频:media_id; 文本:文本消息</param>
                /// <param name="type"></param>
                /// <param name="touser"></param>
                /// <param name="videoTitle"></param>
                /// <param name="videoDesc"></param>
                /// <returns>success:{"errcode":0,"errmsg":"send job submission success","msg_id":34182}</returns>
                public static string Send(string access_token, string content, WeChatArtcleType type, IEnumerable<string> touser, string videoTitle, string videoDesc)
                {
                    var url = string.Format("https://api.weixin.qq.com/cgi-bin/message/mass/send?access_token={0}", access_token);
                    var builder = new StringBuilder();
                    builder.Append("{")
                           .Append('"' + "touser" + '"' + ":")
                           .Append("[");
                    foreach (var t in touser)
                    {
                        builder.Append('"' + t + '"').Append(",");
                    }
                    builder.Append("],");

                    switch (type)
                    {
                        case WeChatArtcleType.News:
                            builder.Append('"' + "mpnews" + '"' + ":")
                                                   .Append("{")
                                                   .Append('"' + "media_id" + '"' + ":").Append(content)
                                                   .Append("},")
                                      .Append('"' + "msgtype" + '"' + ":").Append("mpnews");
                            break;
                        case WeChatArtcleType.Text:
                            builder.Append('"' + "text" + '"' + ":")
                                                   .Append("{")
                                                   .Append('"' + "content" + '"' + ":").Append(content)
                                                   .Append("},")
                                      .Append('"' + "msgtype" + '"' + ":").Append("text");
                            break;
                        case WeChatArtcleType.Voice:
                            builder.Append('"' + "voice" + '"' + ":")
                                                   .Append("{")
                                                   .Append('"' + "media_id" + '"' + ":").Append(content)
                                                   .Append("},")
                                      .Append('"' + "msgtype" + '"' + ":").Append("voice");
                            break;
                        case WeChatArtcleType.Image:
                            builder.Append('"' + "image" + '"' + ":")
                                                   .Append("{")
                                                   .Append('"' + "media_id" + '"' + ":").Append(content)
                                                   .Append("},")
                                      .Append('"' + "msgtype" + '"' + ":").Append("image");
                            break;
                        case WeChatArtcleType.Video:
                            builder.Append('"' + "video" + '"' + ":")
                                                   .Append("{")
                                                   .Append('"' + "media_id" + '"' + ":").Append(content).Append(",")
                                                   .Append('"' + "title" + '"' + ":").Append(videoTitle).Append(",")
                                                   .Append('"' + "description" + '"' + ":").Append(videoDesc)
                                                   .Append("},")
                                      .Append('"' + "msgtype" + '"' + ":").Append("video");
                            break;
                    }
                    builder.Append("}");
                    var result = NetnrCore.HttpTo.Post(url, builder.ToString());
                    return result;
                }

                /// <summary>
                /// 请注意，只有已经发送成功的消息才能删除删除消息只是将消息的图文详情页失效，已经收到的用户，还是能在其本地看到消息卡片。 
                /// 另外，删除群发消息只能删除图文消息和视频消息，其他类型的消息一经发送，无法删除。
                /// </summary>
                /// <param name="access_token"></param>
                /// <param name="msg_id">发送出去的消息ID</param>
                /// <returns>success: {"errcode":0,"errmsg":"ok"}</returns>
                public static string Delete(string access_token, string msg_id)
                {
                    var url = string.Format("https://api.weixin.qq.com/cgi-bin/message/mass/delete?access_token={0}", access_token);
                    var builder = new StringBuilder();
                    builder.Append("{")
                           .Append('"' + "msg_id" + '"' + ":").Append(msg_id)
                           .Append("}");
                    var result = NetnrCore.HttpTo.Post(url, builder.ToString());
                    return result;
                }

                /// <summary>
                /// 预览接口【订阅号与服务号认证后均可用】
                /// 开发者可通过该接口发送消息给指定用户，在手机端查看消息的样式和排版。
                /// </summary>
                /// <param name="access_token"></param>
                /// <param name="openid">接收消息用户对应该公众号的openid</param>
                /// <param name="content">图文消息,语音,图片,视频:media_id(与根据分组群发中的media_id相同); 文本:文本消息</param>
                /// <param name="type"></param>
                /// <returns></returns>
                public static string Preview(string access_token, string openid, string content, WeChatArtcleType type)
                {
                    var url = string.Format("https://api.weixin.qq.com/cgi-bin/message/mass/preview?access_token={0}", access_token);
                    var builder = new StringBuilder();
                    builder.Append("{")
                           .Append('"' + "touser" + '"' + ":")
                           .Append(openid).Append(",");
                    switch (type)
                    {
                        case WeChatArtcleType.News:
                            builder.Append('"' + "mpnews" + '"' + ":")
                                                   .Append("{")
                                                   .Append('"' + "media_id" + '"' + ":").Append(content)
                                                   .Append("},")
                                      .Append('"' + "msgtype" + '"').Append("mpnews");
                            break;
                        case WeChatArtcleType.Text:
                            builder.Append('"' + "text" + '"' + ":")
                                                   .Append("{")
                                                   .Append('"' + "content" + '"' + ":").Append(content)
                                                   .Append("},")
                                      .Append('"' + "msgtype" + '"').Append("text");
                            break;
                        case WeChatArtcleType.Voice:
                            builder.Append('"' + "voice" + '"' + ":")
                                                   .Append("{")
                                                   .Append('"' + "media_id" + '"' + ":").Append(content)
                                                   .Append("},")
                                      .Append('"' + "msgtype" + '"').Append("voice");
                            break;
                        case WeChatArtcleType.Image:
                            builder.Append('"' + "image" + '"' + ":")
                                                   .Append("{")
                                                   .Append('"' + "media_id" + '"' + ":").Append(content)
                                                   .Append("},")
                                      .Append('"' + "msgtype" + '"').Append("image");
                            break;
                        case WeChatArtcleType.Video:
                            builder.Append('"' + "video" + '"' + ":")
                                                   .Append("{")
                                                   .Append('"' + "media_id" + '"' + ":").Append(content)
                                                   .Append("},")
                                      .Append('"' + "msgtype" + '"').Append("video");
                            break;
                    }
                    builder.Append("}");
                    var result = NetnrCore.HttpTo.Post(url, builder.ToString());
                    return result;
                }

                /// <summary>
                /// 查询群发消息发送状态【订阅号与服务号认证后均可用】
                /// 由于群发任务提交后，群发任务可能在一定时间后才完成，因此，群发接口调用时，仅会给出群发任务是否提交成功的提示，
                /// 
                /// 若群发任务提交成功，则在群发任务结束时，会向开发者在公众平台填写的开发者URL（callback URL）推送事件。
                /// 参见 WeChatExecutor.cs: MASSSENDJOBFINISH Event
                /// </summary>
                /// <param name="access_token"></param>
                /// <param name="msg_id"></param>
                /// <returns>success: { "msg_id":201053012,"msg_status":"SEND_SUCCESS"}
                /// “send success”或“send fail”或“err(num)” 
                ///send success时，也有可能因用户拒收公众号的消息、系统错误等原因造成少量用户接收失败。
                ///err(num)是审核失败的具体原因，可能的情况如下：err(10001)涉嫌广告, err(20001)涉嫌政治, err(20004)涉嫌社会,
                ///err(20002)涉嫌色情, err(20006)涉嫌违法犯罪,err(20008)涉嫌欺诈, err(20013)涉嫌版权, err(22000)涉嫌互推(互相宣传), err(21000)涉嫌其他
                /// </returns>
                public static string Get(string access_token, string msg_id)
                {
                    var url = string.Format("https://api.weixin.qq.com/cgi-bin/message/mass/get?access_token={0}", access_token);
                    var builder = new StringBuilder();
                    builder.Append("{")
                           .Append('"' + "msg_id" + '"' + ":").Append(msg_id)
                           .Append("}");
                    var result = NetnrCore.HttpTo.Post(url, builder.ToString());
                    return result;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public class Template
            {
                /// <summary>
                /// 发送模板消息
                /// 在模版消息发送任务完成后，微信服务器会将是否送达成功作为通知，发送到开发者中心中填写的服务器配置地址中。
                /// 参见WeChatExecutor.cs TEMPLATESENDJOBFINISH Event
                /// </summary>
                /// <param name="access_token"></param>
                /// <param name="content">模板消息体,由于模板众多,且结构不一，请开发者自行按照模板自行构建模板消息体,模板消息体为json字符串,请登录微信公众号后台查看</param>
                /// <returns>  { "errcode":0,"errmsg":"ok", "msgid":200228332}
                /// </returns>
                public static string Send(string access_token, object content)
                {
                    var url = string.Format("https://api.weixin.qq.com/cgi-bin/message/template/send?access_token={0}", access_token);
                    var result = NetnrCore.HttpTo.Post(url, NetnrCore.ToJson(content));
                    return result;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class User
        {
            /// <summary>
            /// 获取用户基本信息（包括UnionID机制）
            /// 注意：如果开发者有在多个公众号，或在公众号、移动应用之间统一用户帐号的需求，
            /// 需要前往微信开放平台（open.weixin.qq.com）绑定公众号后，才可利用UnionID机制来满足上述需求。
            /// </summary>
            /// <param name="access_token"></param>
            /// <param name="openId"></param>
            /// <returns>UnionID机制的返回值中将包含“unionid”</returns>
            public static string info(string access_token, string openId)
            {
                var result = NetnrCore.HttpTo.Get(string.Format("https://api.weixin.qq.com/cgi-bin/user/info?access_token={0}&openid={1}&lang=zh_CN", access_token, openId));
                return result;
            }

            /// <summary>
            /// 获取关注者列表
            /// </summary>
            /// <param name="access_token"></param>
            /// <param name="nextOpenId">第一个拉取的OPENID，不填默认从头开始拉取</param>
            /// <returns></returns>
            public static string Get(string access_token, string nextOpenId)
            {
                var result = NetnrCore.HttpTo.Get(string.Format("https://api.weixin.qq.com/cgi-bin/user/get?access_token={0}&next_openid={1}", access_token, nextOpenId));

                return result;
            }

            /// <summary>
            /// 
            /// </summary>
            public class Info
            {
                /// <summary>
                /// 设置备注名
                /// </summary>
                /// <param name="access_token">调用接口凭证</param>
                /// <param name="openid">用户唯一标识符</param>
                /// <param name="remark">新的备注名，长度必须小于30字符</param>
                /// <returns></returns>
                public static string UpdateRemark(string access_token, string openid, string remark)
                {
                    var builder = new StringBuilder();
                    builder.Append("{")
                        .Append('"' + "openid" + '"' + ":").Append(openid).Append(",")
                        .Append('"' + "remark" + '"' + ":").Append(remark)
                        .Append("}");

                    var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/cgi-bin/user/info/updateremark?access_token={0}", access_token), builder.ToString());
                    return result;
                }
            }
        }

        /// <summary>
        /// 多客服功能
        /// http://mp.weixin.qq.com/wiki/5/ae230189c9bd07a6b221f48619aeef35.html
        /// 开发者可根据用户发给公众号的消息内容，选择是转发给客服还是直接回复,
        /// 如果是转发给客服，调用本ＡＰＩ创建客服消息后回传给微信服务器即可
        /// PC客户端自定义插件接口无ＡＰＩ包装!!!
        /// </summary>
        public class CustomService
        {
            /// <summary>
            /// 获取客服基本信息
            /// </summary>
            /// <param name="access_token"></param>
            /// <returns>success: {"errcode" : 0,"errmsg" : "ok",}； 其他代码请使用ExplainCode解析返回代码含义 </returns>
            public static string GetKFList(string access_token)
            {
                var url = string.Format("https://api.weixin.qq.com/cgi-bin/customservice/getkflist?access_token={0}", access_token);
                var result = NetnrCore.HttpTo.Get(url);
                return result;
            }

            /// <summary>
            /// 获取在线客服接待信息
            /// kf_account	完整客服账号，格式为：账号前缀@公众号微信号
            /// status	客服在线状态 1：pc在线，2：手机在线。若pc和手机同时在线则为 1+2=3
            /// kf_id	客服工号
            /// auto_accept	客服设置的最大自动接入数
            /// accepted_case	客服当前正在接待的会话数
            /// </summary>
            /// <param name="access_token"></param>
            /// <returns>success: {"errcode" : 0,"errmsg" : "ok",}； 其他代码请使用ExplainCode解析返回代码含义 </returns>
            public static string GetOnlineKFList(string access_token)
            {
                var url = string.Format("https://api.weixin.qq.com/cgi-bin/customservice/getonlinekflist?access_token={0}", access_token);
                var result = NetnrCore.HttpTo.Get(url);
                return result;
            }

            /// <summary>
            /// 构建多客服消息，用于回复微信服务器提交过来的用户消息
            /// </summary>
            /// <param name="toUserName">发送方帐号（一个OpenID）</param>
            /// <param name="fromUserName">开发者微信号</param>
            /// <returns></returns>
            public static string BuildTransferCustomerServiceMessage(string toUserName, string fromUserName)
            {
                return string.Format("<xml>" +
               "<ToUserName><![CDATA[{0}]]></ToUserName>" +
               "<FromUserName><![CDATA[{1}]]></FromUserName>" +
               "<CreateTime>{2}</CreateTime>" +
               "<MsgType><![CDATA[transfer_customer_service]]></MsgType>" +
               "</xml>", toUserName, Util.Timestamp(), fromUserName);
            }

            /// <summary>
            /// 构建消息转发到指定客服的多客服消息，用于回复微信服务器提交过来的用户消息
            /// </summary>
            /// <param name="toUserName">发送方帐号（一个OpenID）</param>
            /// <param name="fromUserName">开发者微信号</param>
            /// <param name="kfAccount">指定会话接入的客服账号</param>
            /// <returns></returns>
            public static string BuildTransferCustomerServiceMessage(string toUserName, string fromUserName, string kfAccount)
            {
                return string.Format("<xml>" +
               "<ToUserName><![CDATA[{0}]]></ToUserName>" +
               "<FromUserName><![CDATA[{1}]]></FromUserName>" +
               "<CreateTime>{2}</CreateTime>" +
               "<MsgType><![CDATA[transfer_customer_service]]></MsgType>" +
               "<TransInfo><KfAccount><![CDATA[{3}]]></KfAccount></TransInfo>" +
               "</xml>", toUserName, Util.Timestamp(), fromUserName, kfAccount);
            }

            /// <summary>
            /// 获取客服聊天记录接口
            /// </summary>
            /// <param name="access_token"></param>
            /// <param name="openid"></param>
            /// <param name="starttime"></param>
            /// <param name="endtime"></param>
            /// <param name="pagesize"></param>
            /// <param name="pageindex"></param>
            /// <returns></returns>
            public static string GetRecord(string access_token, string openid, int starttime, int endtime, int pagesize, int pageindex)
            {
                var builder = new StringBuilder();
                builder
                    .Append("{")
                    .Append('"' + "starttime" + '"' + ":").Append(starttime).Append(",")
                    .Append('"' + "endtime" + '"' + ":").Append(endtime).Append(",")
                    .Append('"' + "openid" + '"' + ":").Append(openid).Append(",")
                    .Append('"' + "pagesize" + '"' + ":").Append(pagesize).Append(",")
                    .Append('"' + "pageindex" + '"' + ":").Append(pageindex)
                    .Append("}");

                var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/cgi-bin/customservice/getrecord?access_token={0}", access_token), builder.ToString());
                return result;
            }

            /// <summary>
            /// 解释聊天记录的opercode的含义
            /// </summary>
            /// <param name="opercode"></param>
            /// <returns></returns>
            public static string ExplainOpercode(int opercode)
            {
                switch (opercode)
                {
                    case 1000:
                        return "创建未接入会话";
                    case 1001:
                        return "接入会话";
                    case 1002:
                        return "主动发起会话";
                    case 1004:
                        return "关闭会话";
                    case 1005:
                        return "抢接会话";
                    case 2001:
                        return "公众号收到消息";
                    case 2002:
                        return "客服发送消息";
                    case 2003:
                        return "客服收到消息";
                    default:
                        return "";
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public class KFAccount
            {
                /// <summary>
                /// 设置客服信息
                /// </summary>
                /// <param name="access_token"></param>
                /// <param name="kf_account">完整客服账号，格式为：账号前缀@公众号微信号，账号前缀最多10个字符，必须是英文或者数字字符。如果没有公众号微信号，请前往微信公众平台设置。</param>
                /// <param name="nickname">客服昵称，最长6个汉字或12个英文字符</param>
                /// <param name="pswmd5">客服账号登录密码，格式为密码明文的32位加密MD5值</param>
                /// <returns>success: {"errcode" : 0,"errmsg" : "ok",}； 其他代码请使用ExplainCode解析返回代码含义 </returns>
                public static string Update(string access_token, string kf_account, string nickname, string pswmd5)
                {
                    var url = string.Format("https://api.weixin.qq.com/customservice/kfaccount/update?access_token={0}", access_token);
                    var builder = new StringBuilder();
                    builder
                        .Append("{")
                        .Append('"' + "kf_account" + '"' + ":").Append(kf_account).Append(",")
                        .Append('"' + "nickname" + '"' + ":").Append(nickname).Append(",")
                        .Append('"' + "password" + '"' + ":").Append(pswmd5)
                        .Append("}");

                    var result = NetnrCore.HttpTo.Post(url, builder.ToString());
                    return result;
                }

                /// <summary>
                /// 上传客服头像
                /// </summary>
                /// <param name="access_token"></param>
                /// <param name="kf_account">完整客服账号，格式为：账号前缀@公众号微信号</param>
                /// <param name="icon"></param>
                /// <returns>success: {"errcode" : 0,"errmsg" : "ok",}； 其他代码请使用ExplainCode解析返回代码含义 </returns>
                public static string UploadHeadImg(string access_token, string kf_account, string icon)
                {
                    var url = string.Format("http://api.weixin.qq.com/customservice/kfacount/uploadheadimg?access_token={0}&kf_account={1}", access_token, kf_account);

                    var fsRead = new FileStream(icon, FileMode.Open, FileAccess.Read);

                    int fsLen = (int)fsRead.Length;
                    byte[] heByte = new byte[fsLen];
                    fsRead.Read(heByte, 0, heByte.Length);
                    string myStr = Encoding.UTF8.GetString(heByte);

                    var result = NetnrCore.HttpTo.Post(url, myStr);
                    return result;
                }

                /// <summary>
                /// 删除客服账号
                /// </summary>
                /// <param name="access_token"></param>
                /// <param name="kf_account"></param>
                /// <returns>success: {"errcode" : 0,"errmsg" : "ok",}； 其他代码请使用ExplainCode解析返回代码含义 </returns>
                public static string Del(string access_token, string kf_account)
                {
                    var url = string.Format("https://api.weixin.qq.com/customservice/kfaccount/del?access_token={0}&kf_account={1}", access_token, kf_account);
                    var result = NetnrCore.HttpTo.Get(url);
                    return result;
                }

                /// <summary>
                /// 解释客服管理接口返回码说明
                /// </summary>
                /// <param name="code"></param>
                /// <returns></returns>
                public static string ExplainCode(int code)
                {
                    switch (code)
                    {
                        case 0:
                            return "成功(no error)";
                        case 61451:
                            return "参数错误(invalid parameter)";
                        case 61452:
                            return "无效客服账号(invalid kf_account)";
                        case 61453:
                            return "账号已存在(kf_account exsited)";
                        case 61454:
                            return "账号名长度超过限制(前缀10个英文字符)(invalid kf_acount length)";
                        case 61455:
                            return "账号名包含非法字符(英文+数字)(illegal character in kf_account)";
                        case 61456:
                            return "账号个数超过限制(10个客服账号)(kf_account count exceeded)";
                        case 61457:
                            return "无效头像文件类型(invalid file type)";
                        default:
                            return "";
                    }
                }
            }
        }
    }
}
