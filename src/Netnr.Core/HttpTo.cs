using System.IO;
using System.Net;
using System.Text;

namespace Netnr.Core
{
    /// <summary>
    /// HTTP请求
    /// </summary>
    public class HttpTo
    {
        /// <summary>
        /// HttpWebRequest对象
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="type">请求类型，默认GET</param>
        /// <param name="data">发送数据，非GET、DELETE请求</param>
        /// <returns></returns>
        public static HttpWebRequest HWRequest(string url, string type = "GET", byte[] data = null)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = type;
            request.KeepAlive = true;
            request.AllowAutoRedirect = true;
            request.MaximumAutomaticRedirections = 4;
            request.Timeout = short.MaxValue * 3;//MS
            request.ContentType = "application/x-www-form-urlencoded";

            //发送内容
            if (type != "GET" && data != null)
            {
                request.ContentLength = data.Length;
                Stream outputStream = request.GetRequestStream();
                outputStream.Write(data, 0, data.Length);
                outputStream.Close();
            }

            return request;
        }

        /// <summary>
        /// HTTP请求
        /// </summary>
        /// <param name="request">HttpWebRequest对象</param>
        /// <param name="charset">编码，默认utf-8</param>
        /// <param name="response">输出</param>
        /// <returns></returns>
        public static StreamReader Stream(HttpWebRequest request, ref HttpWebResponse response, string charset = "utf-8")
        {
            response = (HttpWebResponse)request.GetResponse();

            Stream responseStream = response.GetResponseStream();
            if (string.Compare(response.ContentEncoding, "gzip", true) >= 0)
                responseStream = new System.IO.Compression.GZipStream(responseStream, System.IO.Compression.CompressionMode.Decompress);

            return string.IsNullOrEmpty(charset) ?
                new StreamReader(responseStream) : new StreamReader(responseStream, Encoding.GetEncoding(charset));
        }

        /// <summary>
        /// HTTP请求
        /// </summary>
        /// <param name="request">HttpWebRequest对象</param>
        /// <param name="fullFilePath">存储完整路径</param>
        /// <param name="charset">编码，默认utf-8</param>
        /// <returns></returns>
        public static void DownloadSave(HttpWebRequest request, string fullFilePath, string charset = "utf-8")
        {
            HttpWebResponse response = null;
            var stream = Stream(request, ref response, charset);

            using MemoryStream ms = new();
            stream.BaseStream.CopyTo(ms);
            var bytes = ms.ToArray();

            using var fs = new FileStream(fullFilePath, FileMode.Create);
            fs.Write(bytes, 0, bytes.Length);
            fs.Flush();
        }

        /// <summary>
        /// HTTP请求
        /// </summary>
        /// <param name="request">HttpWebRequest对象</param>
        /// <param name="charset">编码，默认utf-8</param>
        /// <returns></returns>
        public static string Url(HttpWebRequest request, string charset = "utf-8")
        {
            HttpWebResponse response = null;
            var stream = Stream(request, ref response, charset);
            return stream.ReadToEnd();
        }

        /// <summary>
        /// GET请求
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="charset">编码，默认utf-8</param>
        /// <returns></returns>
        public static string Get(string url, string charset = "utf-8")
        {
            var request = HWRequest(url, "GET", null);
            return Url(request, charset);
        }

        /// <summary>
        /// POST请求
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="data">发送数据</param>
        /// <param name="charset">编码，默认utf-8</param>
        /// <returns></returns>
        public static string Post(string url, string data, string charset = "utf-8")
        {
            var request = HWRequest(url, "POST", Encoding.GetEncoding(charset).GetBytes(data));
            return Url(request, charset);
        }

        /// <summary>
        /// POST请求
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="bytes">发送数据</param>
        /// <param name="charset">编码，默认utf-8</param>
        /// <returns></returns>
        public static string Post(string url, byte[] bytes, string charset = "utf-8")
        {
            var request = HWRequest(url, "POST", bytes);
            return Url(request, charset);
        }
    }
}