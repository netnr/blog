using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.IO.Compression;

namespace Netnr.Core
{
    /// <summary>
    /// 压缩
    /// </summary>
    public class ZipTo
    {
        /// <summary>
        /// 创建打包
        /// </summary>
        /// <param name="pathName">文件完整路径-（可选）包内文件名</param>
        /// <param name="zipPath">zip 完整路径，默认添加的第一个文件同目录下</param>
        public static string Create(Dictionary<string, string> pathName, string zipPath = null)
        {
            if (string.IsNullOrWhiteSpace(zipPath))
            {
                var dn = Path.GetDirectoryName(pathName.Keys.First());
                zipPath = Path.Combine(dn, Path.GetFileName(dn) + ".zip");
            }
            using ZipArchive zip = ZipFile.Open(zipPath, File.Exists(zipPath) ? ZipArchiveMode.Update : ZipArchiveMode.Create);

            foreach (var path in pathName.Keys)
            {
                var name = pathName[path];
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = Path.GetFileName(path);
                }

                zip.CreateEntryFromFile(path, name);
            }

            return zipPath;
        }

        /// <summary>
        /// 创建打包
        /// </summary>
        /// <param name="fullPath">需打包的文件夹完整路径</param>
        /// <param name="zipPath">zip 完整路径，默认文件夹同目录</param>
        public static string Create(string fullPath, string zipPath = null)
        {
            if (string.IsNullOrWhiteSpace(zipPath))
            {
                zipPath = Path.Combine(Path.GetDirectoryName(fullPath), Path.GetFileName(fullPath) + ".zip");
            }

            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }
            ZipFile.CreateFromDirectory(fullPath, zipPath, CompressionLevel.Optimal, false);

            return zipPath;
        }

        /// <summary>
        /// 解压提取
        /// </summary>
        /// <param name="zipPath">zip 完整路径</param>
        /// <param name="dirName">文件完整路径-包内文件名（可选）</param>
        public static string Extract(string zipPath, string dirName = null)
        {
            if (string.IsNullOrWhiteSpace(dirName))
            {
                dirName = zipPath.Replace(Path.GetExtension(zipPath), "");
            }

            ZipFile.ExtractToDirectory(zipPath, dirName);

            return dirName;
        }
    }
}
