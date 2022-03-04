using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Netnr.Core
{
    /// <summary>
    /// 文件
    /// </summary>
    public class FileTo
    {
        /// <summary>
        /// 流写入
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="fileFullPath">文件完整物理路径</param>
        /// <param name="e">编码</param>
        /// <param name="isAppend">默认追加，false覆盖</param>
        public static void WriteText(string content, string fileFullPath, Encoding e, bool isAppend = true)
        {
            var dn = Path.GetDirectoryName(fileFullPath);
            //检测目录
            if (!Directory.Exists(dn))
            {
                Directory.CreateDirectory(dn);
            }

            //打开方式
            var fm = (!File.Exists(fileFullPath) || !isAppend) ? FileMode.Create : FileMode.Append;

            using var fs = new FileStream(fileFullPath, fm);
            //流写入
            using var sw = new StreamWriter(fs, e);
            sw.WriteLine(content);
            sw.Flush();
            sw.Close();
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="content"></param>
        /// <param name="fileFullPath">文件完整物理路径</param>
        /// <param name="isAppend"></param>
        public static void WriteText(string content, string fileFullPath, bool isAppend = true)
        {
            WriteText(content, fileFullPath, Encoding.UTF8, isAppend);
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="fileFullPath">文件完整物理路径</param>
        /// <param name="e">编码 默认UTF8</param>
        /// <returns></returns>
        public static string ReadText(string fileFullPath, Encoding e = null)
        {
            var result = string.Empty;

            if (File.Exists(fileFullPath))
            {
                if (e == null)
                {
                    e = Encoding.UTF8;
                }

                result = File.ReadAllText(fileFullPath, e);
            }

            return result;
        }

        /// <summary>
        /// 拷贝目录
        /// </summary>
        /// <param name="source">源目录</param>
        /// <param name="target">新目录</param>
        /// <param name="ignoreFolder">忽略文件夹</param>
        public static void CopyDirectory(DirectoryInfo source, DirectoryInfo target, List<string> ignoreFolder = null)
        {
            if (source.FullName.Equals(target.FullName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (ignoreFolder != null && ignoreFolder.Any(x => target.FullName.EndsWith(x)))
            {
                return;
            }

            if (Directory.Exists(target.FullName) == false)
            {
                Directory.CreateDirectory(target.FullName);
            }

            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }

            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                if (ignoreFolder != null && ignoreFolder.Any(x => x == diSourceSubDir.Name))
                {
                    continue;
                }
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyDirectory(diSourceSubDir, nextTargetSubDir, ignoreFolder);
            }
        }

        /// <summary>
        /// 拷贝目录
        /// </summary>
        /// <param name="sourceDirectory">源目录</param>
        /// <param name="targetDirectory">新目录</param>
        /// <param name="ignoreFolder">忽略文件夹</param>
        public static void CopyDirectory(string sourceDirectory, string targetDirectory, List<string> ignoreFolder = null)
        {
            DirectoryInfo diSource = new(sourceDirectory);
            DirectoryInfo diTarget = new(targetDirectory);

            CopyDirectory(diSource, diTarget, ignoreFolder);
        }
    }
}