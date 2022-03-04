using System.Xml;

namespace Netnr.WeChat
{
    /// <summary>
    /// 
    /// </summary>
    public static class Extend
    {
        /// <summary>
        /// 获取XmlDocument的内容
        /// </summary>
        /// <param name="xmlDocument"></param>
        /// <param name="nodeName">节点名称</param>
        /// <returns></returns>
        public static string GetText(this XmlDocument xmlDocument, string nodeName)
        {
            string result = xmlDocument.SelectSingleNode("//" + nodeName).InnerText;
            return result;
        }
    }
}