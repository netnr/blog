namespace Netnr.WeChat.Entities
{
    /// <summary>
    /// 回复用户的消息类型
    /// </summary>
    public enum WeChatArtcleType
    {
        /// <summary>
        /// 图文消息
        /// </summary>
        News,
        /// <summary>
        /// 文本
        /// </summary>
        Text,
        /// <summary>
        /// 语音
        /// </summary>
        Voice,
        /// <summary>
        /// 图片
        /// </summary>
        Image,
        /// <summary>
        /// 视频
        /// </summary>
        Video,
        /// <summary>
        /// 音乐
        /// </summary>
        Music
    }

    /// <summary>
    /// 图文消息
    /// </summary>
    public class WeChatArtcle
    {
        /// <summary>
        /// 图文消息缩略图的media_id，可以在基础支持-上传多媒体文件接口中获得
        /// </summary>
        public string thumb_media_id { set; get; }

        /// <summary>
        /// 图文消息的作者
        /// </summary>
        public string author { set; get; }

        /// <summary>
        /// 图文消息的标题
        /// </summary>
        public string title { set; get; }

        /// <summary>
        /// 在图文消息页面点击“阅读原文”后的页面
        /// </summary>
        public string content_source_url { set; get; }

        /// <summary>
        /// 图文消息页面的内容，支持HTML标签
        /// </summary>
        public string content { set; get; }

        /// <summary>
        /// 图文消息的描述
        /// </summary>
        public string digest { set; get; }

        /// <summary>
        /// 是否显示封面，1为显示，0为不显示
        /// </summary>
        public string show_cover_pic { set; get; }
    }
}
