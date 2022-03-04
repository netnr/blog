namespace Netnr.Blog.Application
{
    /// <summary>
    /// 枚举
    /// </summary>
    public class EnumService
    {
        /// <summary>
        /// 回复分类
        /// </summary>
        public enum ReplyType
        {
            /// <summary>
            /// 文章
            /// </summary>
            UserWriting,
            /// <summary>
            /// 尬服
            /// </summary>
            GuffRecord
        }

        /// <summary>
        /// 消息分类
        /// </summary>
        public enum MessageType
        {
            /// <summary>
            /// 文章
            /// </summary>
            UserWriting,
            /// <summary>
            /// 尬服
            /// </summary>
            GuffRecord
        }

        /// <summary>
        /// 关联分类
        /// </summary>
        public enum ConnectionType
        {
            /// <summary>
            /// 文章
            /// </summary>
            UserWriting,
            /// <summary>
            /// 尬服
            /// </summary>
            GuffRecord
        }
    }
}
