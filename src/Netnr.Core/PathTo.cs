namespace Netnr.Core
{
    /// <summary>
    /// 路径
    /// </summary>
    public class PathTo
    {
        /// <summary>
        /// 路径结合，默认 / 拼接
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string Combine(params string[] args)
        {
            var path = string.Empty;
            foreach (var arg in args)
            {
                if (!string.IsNullOrWhiteSpace(arg))
                {
                    if (path == string.Empty)
                    {
                        path = arg.Trim();
                    }
                    else
                    {
                        var tsarg = arg.Trim().TrimStart('/');
                        path += (path.EndsWith("/") || path.EndsWith("\\")) ? tsarg : '/' + tsarg;
                    }
                }
            }
            return path;
        }
    }
}