using System;
using System.Xml;

namespace Netnr.WeChat
{
    /// <summary>
    /// 
    /// </summary>
    public enum WeChatMessageType
    {
        /// <summary>
        /// 文本
        /// </summary>
        Text,
        /// <summary>
        /// 地理位置
        /// </summary>
        Location,
        /// <summary>
        /// 图片
        /// </summary>
        Image,
        /// <summary>
        /// 语音
        /// </summary>
        Voice,
        /// <summary>
        /// 视频
        /// </summary>
        Video,
        /// <summary>
        /// 链接
        /// </summary>
        Link,
        /// <summary>
        /// 事件推送
        /// </summary>
        Event
    }

    /// <summary>
    /// 
    /// </summary>
    public class WeChatMessage
    {
        /// <summary>
        /// 
        /// </summary>
        public virtual WeChatMessageType Type { set; get; }

        /// <summary>
        /// 
        /// </summary>
        public virtual XmlDocument Body { set; get; }

        /// <summary>
        /// 解析微信服务器推送的消息
        /// http://mp.weixin.qq.com/wiki/index.php?title=%E6%8E%A5%E6%94%B6%E6%99%AE%E9%80%9A%E6%B6%88%E6%81%AF
        /// http://mp.weixin.qq.com/wiki/index.php?title=%E6%8E%A5%E6%94%B6%E4%BA%8B%E4%BB%B6%E6%8E%A8%E9%80%81
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static WeChatMessage Parse(string message)
        {
            var msg = new WeChatMessage();
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(message);
            msg.Body = xmlDoc;
            string msgType = msg.Body.SelectSingleNode("//MsgType").InnerText.ToLower();
            if (Enum.TryParse(msgType, true, out WeChatMessageType mtype))
            {
                msg.Type = mtype;
            }
            else
            {
                throw new Exception("does not support this message type:" + msgType);
            }
            return msg;
        }
    }
}