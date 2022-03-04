#if Full || IpArea

using System;
using System.IO;
using System.Linq;
using Netnr.Core;
using System.Collections.Generic;

namespace Netnr.SharedIpArea
{
    /// <summary>
    /// IP 归属地查询
    /// </summary>
    public class IpAreaTo
    {
        /// <summary>
        /// 数据库完整物理路径
        /// </summary>
        public string DbPath = PathTo.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "ip2region.db");

        public IpAreaTo()
        {
            if (!File.Exists(DbPath))
            {
                try
                {
                    HttpTo.DownloadSave(HttpTo.HWRequest("https://raw.githubusercontent.com/lionsoul2014/ip2region/master/data/ip2region.db"), DbPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dbPath">指定 ip2region 数据库文件路径</param>
        public IpAreaTo(string dbPath = null)
        {
            if (dbPath != null)
            {
                DbPath = dbPath;
            }
        }

        /// <summary>
        /// 解析 IP 归属地
        /// </summary>
        /// <param name="dicIp">索引：IP地址</param>
        public void Parse(ref Dictionary<int, string> dicIp)
        {
            var hasdb = File.Exists(DbPath);
            IP2Region.DbSearcher ipds = null;
            if (hasdb)
            {
                ipds = new IP2Region.DbSearcher(DbPath);
            }
            var listKey = dicIp.Keys.ToList();
            foreach (var key in listKey)
            {
                if (hasdb)
                {
                    var val = dicIp[key];
                    if (!string.IsNullOrWhiteSpace(val))
                    {
                        try
                        {
                            var ips = val.Split(',');
                            var ipi = new List<string>();

                            foreach (var ip in ips)
                            {
                                //内容格式：国家|区域|省份|市|运营商。无数据默认为0。
                                var listIpInfo = ipds.MemorySearch(ip.Split(':')[0].Trim().Replace("::1", "127.0.0.1")).Region.Split('|').ToList();

                                listIpInfo.RemoveAt(1);
                                listIpInfo = listIpInfo.Where(x => x != "0").Distinct().ToList();

                                ipi.Add(string.Join(",", listIpInfo));
                            }

                            dicIp[key] = string.Join(";", ipi);
                        }
                        catch (Exception)
                        {
                            dicIp[key] = "fail";
                        }
                    }
                    else
                    {
                        dicIp[key] = "";
                    }
                }
                else
                {
                    dicIp[key] = "";
                }
            }

            ipds?.Dispose();
        }

        /// <summary>
        /// 解析 IP 归属地
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <returns></returns>
        public string Parse(string ip)
        {
            var dic = new Dictionary<int, string>
            {
                {1,ip }
            };
            Parse(ref dic);
            return dic[1];
        }
    }
}

#endif