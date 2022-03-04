namespace Netnr.WeChat
{
    /// <summary>
    /// 
    /// </summary>
    public interface IWeChatExecutor
    {
        /// <summary>
        /// 接受消息后返回XML
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        string Execute(WeChatMessage message);
    }
}
