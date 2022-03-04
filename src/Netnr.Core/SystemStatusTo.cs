using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Netnr.Core
{
    /// <summary>
    /// 系统状态
    /// </summary>
    public class SystemStatusTo
    {
        /// <summary>
        /// 确定当前操作系统是否为64位操作系统
        /// </summary>
        public bool Is64BitOperatingSystem { get; set; } = Environment.Is64BitOperatingSystem;
        /// <summary>
        /// 获取此本地计算机的NetBIOS名称
        /// </summary>
        public string MachineName { get; set; } = Environment.MachineName;
        /// <summary>
        /// 获取当前平台标识符和版本号
        /// </summary>
        public OperatingSystem OSVersion { get; set; } = Environment.OSVersion;
        /// <summary>
        /// 获取当前计算机上的处理器数量
        /// </summary>
        public int ProcessorCount { get; set; } = Environment.ProcessorCount;
        /// <summary>
        /// 处理器名称
        /// </summary>
        public string ProcessorName { get; set; }
        /// <summary>
        /// 处理器使用率
        /// </summary>
        public decimal ProcessorUsage { get; set; }
        /// <summary>
        /// 获取系统目录的标准路径
        /// </summary>
        public string SystemDirectory { get; set; } = Environment.SystemDirectory;
        /// <summary>
        /// 获取操作系统的内存页面中的字节数
        /// </summary>
        public int SystemPageSize { get; set; } = Environment.SystemPageSize;
        /// <summary>
        /// 获取自系统启动以来经过的毫秒数
        /// </summary>
        public long TickCount { get; set; }
        /// <summary>
        /// 获取与当前用户关联的网络域名
        /// </summary>
        public string UserDomainName { get; set; } = Environment.UserDomainName;
        /// <summary>
        /// 获取当前登录到操作系统的用户的用户名
        /// </summary>
        public string UserName { get; set; } = Environment.UserName;
        /// <summary>
        /// 获取公共语言运行时的主要，次要，内部和修订版本号
        /// </summary>
        public Version Version { get; set; } = Environment.Version;
        /// <summary>
        /// 获取运行应用程序的.NET安装的名称
        /// </summary>
        public string FrameworkDescription { get; set; } = RuntimeInformation.FrameworkDescription;
        /// <summary>
        /// 获取描述应用程序正在运行的操作系统的字符串
        /// </summary>
        public string OSDescription { get; set; } = RuntimeInformation.OSDescription;
        /// <summary>
        /// 代表操作系统平台
        /// </summary>
        public string OS { get; set; }
        /// <summary>
        /// 总物理内存 B
        /// </summary>
        public long TotalPhysicalMemory { get; set; }
        /// <summary>
        /// 可用物理内存 B
        /// </summary>
        public long FreePhysicalMemory { get; set; }
        /// <summary>
        /// 总交换空间（Linux）B
        /// </summary>
        public long SwapTotal { get; set; }
        /// <summary>
        /// 可用交换空间（Linux）B
        /// </summary>
        public long SwapFree { get; set; }
        /// <summary>
        /// 使用物理内存
        /// </summary>
        public long UsePhysicalMemory { get; set; } = Environment.WorkingSet;
        /// <summary>
        /// 逻辑磁盘 B
        /// </summary>
        public object LogicalDisk { get; set; }
        /// <summary>
        /// 型号
        /// </summary>
        public string Model { get; set; }
        /// <summary>
        /// 操作系统
        /// </summary>
        public string OperatingSystem { get; set; }


        /// <summary>
        /// 构造
        /// </summary>
        public SystemStatusTo()
        {
            OS = GetOSPlatform();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                TotalPhysicalMemory = PlatformForWindows.TotalPhysicalMemory();
                FreePhysicalMemory = PlatformForWindows.FreePhysicalMemory();

                LogicalDisk = PlatformForWindows.LogicalDisk();

                ProcessorName = PlatformForWindows.ProcessorName();
                ProcessorUsage = PlatformForWindows.CPUUsage();

                TickCount = PlatformForWindows.RunTime();

                Model = PlatformForWindows.Model();

                OperatingSystem = PlatformForWindows.OperatingSystem();
            }
            else
            {
                TotalPhysicalMemory = PlatformForLinux.MemInfo("MemTotal:");
                FreePhysicalMemory = PlatformForLinux.MemInfo("MemAvailable:");

                SwapFree = PlatformForLinux.MemInfo("SwapFree:");
                SwapTotal = PlatformForLinux.MemInfo("SwapTotal:");

                LogicalDisk = PlatformForLinux.LogicalDisk();

                ProcessorName = PlatformForLinux.CpuInfo("model name");
                ProcessorUsage = PlatformForLinux.CPUUsage();

                TickCount = PlatformForLinux.RunTime();

                Model = PlatformForLinux.Model();

                OperatingSystem = PlatformForLinux.OperatingSystem();
            }
        }

        /// <summary>
        /// 获取平台
        /// </summary>
        /// <returns></returns>
        public static string GetOSPlatform()
        {
            string osp = string.Empty;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                osp = "Windows";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                osp = "Linux";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                osp = "OSX";
            }

            return osp;
        }

        /// <summary>
        /// WINDOWS
        /// </summary>
        public class PlatformForWindows
        {
            /// <summary>
            /// 获取物理内存 B
            /// </summary>
            /// <returns></returns>
            public static long TotalPhysicalMemory()
            {
                var cmd = "wmic os get TotalVisibleMemorySize /value";
                try
                {
                    var cr = CmdTo.Execute(cmd).CrOutput.Split('=').LastOrDefault().Trim().Split('.').First();
                    long TotalPhysicalMemory = 1024 * long.Parse(cr);

                    return TotalPhysicalMemory;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(cmd);
                    Console.WriteLine(ex);
                }
                return 0;
            }

            /// <summary>
            /// 获取可用内存 B
            /// </summary>
            public static long FreePhysicalMemory()
            {
                var cmd = "wmic os get FreePhysicalMemory /value";
                try
                {
                    var cr = CmdTo.Execute(cmd).CrOutput.Split('=').LastOrDefault().Trim().Split('.').First();
                    long FreePhysicalMemory = 1024 * long.Parse(cr);

                    return FreePhysicalMemory;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(cmd);
                    Console.WriteLine(ex);
                }
                return 0;
            }

            /// <summary>
            /// 获取磁盘信息
            /// </summary>
            /// <returns></returns>
            public static List<object> LogicalDisk()
            {
                var listld = new List<object>();
                var cmd = "wmic logicaldisk where DriveType=3 get FreeSpace,Name,Size,VolumeName";
                try
                {
                    var cr = CmdTo.Execute(cmd).CrOutput.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                    foreach (var item in cr)
                    {
                        var mr = Regex.Match(item, @"(\d+)\s+(\w:)\s+(\d+)\s+(.*)");
                        if (mr.Success)
                        {
                            listld.Add(new
                            {
                                Name = mr.Groups[2].ToString(),
                                VolumeName = mr.Groups[4].ToString().Trim(),
                                Size = long.Parse(mr.Groups[3].ToString()),
                                FreeSpace = long.Parse(mr.Groups[1].ToString()),
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(cmd);
                    Console.WriteLine(ex);
                }
                return listld;
            }

            /// <summary>
            /// 获取处理器名称
            /// </summary>
            /// <returns></returns>
            public static string ProcessorName()
            {
                var cmd = "wmic cpu get Name /value";
                try
                {
                    var cr = CmdTo.Execute(cmd).CrOutput.Split('=')[1].Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0].Trim();

                    return cr;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(cmd);
                    Console.WriteLine(ex);
                }
                return null;
            }

            /// <summary>
            /// 获取CPU使用率 %
            /// </summary>
            /// <returns></returns>
            public static decimal CPUUsage()
            {
                var cmd = "PowerShell \"Get-Counter '\\Processor(_Total)\\% Processor Time'\"";
                try
                {
                    var list = CmdTo.Execute(cmd).CrOutput.Trim().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    var cu = Math.Ceiling(Convert.ToDecimal(list.LastOrDefault().ToString().Trim()));

                    return cu;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(cmd);
                    Console.WriteLine(ex);
                }
                return 0;
            }

            /// <summary>
            /// 运行时长
            /// </summary>
            /// <returns></returns>
            public static long RunTime()
            {
                var cmd = "wmic os get LastBootUpTime /value";
                try
                {
                    var cr = CmdTo.Execute(cmd).CrOutput.Split('=').LastOrDefault().Trim().Split('.').First();
                    cr = cr.Insert(12, ":").Insert(10, ":").Insert(8, " ").Insert(6, "-").Insert(4, "-");
                    _ = DateTime.TryParse(cr, out DateTime startTime);
                    var rt = Convert.ToInt64((DateTime.Now - startTime).TotalMilliseconds);

                    return rt;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(cmd);
                    Console.WriteLine(ex);
                }
                return 0;
            }

            /// <summary>
            /// 获取型号
            /// </summary>
            /// <returns></returns>
            public static string Model()
            {
                var cmd = "wmic csproduct get name /value";
                try
                {
                    var cr = CmdTo.Execute(cmd).CrOutput.Split('=')[1].Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0].Trim();

                    return cr;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(cmd);
                    Console.WriteLine(ex);
                }
                return null;
            }

            /// <summary>
            /// 获取操作系统
            /// </summary>
            /// <returns></returns>
            public static string OperatingSystem()
            {
                var cmd = "wmic os get Caption /value";
                try
                {
                    var cr = CmdTo.Execute(cmd).CrOutput.Split('=')[1].Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0].Trim();

                    return cr;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(cmd);
                    Console.WriteLine(ex);
                }
                return null;
            }
        }

        /// <summary>
        /// Linux系统
        /// </summary>
        public class PlatformForLinux
        {
            /// <summary>
            /// 获取 /proc/meminfo
            /// </summary>
            /// <param name="pkey"></param>
            /// <returns></returns>
            public static long MemInfo(string pkey)
            {
                try
                {
                    var meminfo = FileTo.ReadText("/proc/meminfo");
                    var pitem = meminfo.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(x => x.StartsWith(pkey));

                    var pvalue = 1024 * long.Parse(pitem.Replace(pkey, "").ToLower().Replace("kb", "").Trim());

                    return pvalue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("/proc/meminfo");
                    Console.WriteLine(ex);
                }
                return 0;
            }

            /// <summary>
            /// 获取 /proc/cpuinfo
            /// </summary>
            /// <param name="pkey"></param>
            /// <returns></returns>
            public static string CpuInfo(string pkey)
            {
                try
                {
                    var meminfo = FileTo.ReadText("/proc/cpuinfo") + Environment.NewLine + CmdTo.Execute("lscpu").CrOutput;
                    var pitem = meminfo.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(x => x.StartsWith(pkey, StringComparison.OrdinalIgnoreCase));

                    var pvalue = pitem.Split(':')[1].Trim();

                    return pvalue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("/proc/cpuinfo");
                    Console.WriteLine("lscpu");
                    Console.WriteLine(ex);
                }
                return null;
            }

            /// <summary>
            /// 获取磁盘信息
            /// </summary>
            /// <returns></returns>
            public static List<object> LogicalDisk()
            {
                var listld = new List<object>();
                try
                {
                    var dfresult = CmdTo.Execute("df").CrOutput;
                    var listdev = dfresult.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Where(x => x.StartsWith("/dev/"));
                    foreach (var devitem in listdev)
                    {
                        var dis = devitem.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                        var size = long.Parse(dis[1]) * 1024;
                        //大于 1GB
                        if (size > 1073741824)
                        {
                            listld.Add(new
                            {
                                Name = dis[0],
                                Size = size,
                                FreeSpace = long.Parse(dis[3]) * 1024
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("df");
                    Console.WriteLine(ex);
                }
                return listld;
            }

            /// <summary>
            /// 获取CPU使用率 %
            /// </summary>
            /// <returns></returns>
            public static decimal CPUUsage()
            {
                try
                {
                    var br = CmdTo.Execute("vmstat 1 2").CrOutput;
                    var cpuitems = br.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).LastOrDefault().Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                    var usi = cpuitems.Count - 5;
                    var us = cpuitems[usi];

                    return decimal.Parse(us);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("vmstat 1 2");
                    Console.WriteLine(ex);
                }
                return 0;
            }

            /// <summary>
            /// 运行时长
            /// </summary>
            /// <returns></returns>
            public static long RunTime()
            {
                try
                {
                    var uptime = FileTo.ReadText("/proc/uptime");
                    var pitem = Convert.ToDouble(uptime.Split(' ')[0]);

                    var pvalue = Convert.ToInt64(pitem * 1000); ;

                    return pvalue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("/proc/uptime");
                    Console.WriteLine(ex);
                }
                return 0;
            }

            /// <summary>
            /// 获取型号
            /// </summary>
            /// <returns></returns>
            public static string Model()
            {
                try
                {
                    var model = FileTo.ReadText("/sys/class/dmi/id/product_name")?.Trim();

                    return model;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("/sys/class/dmi/id/product_name");
                    Console.WriteLine(ex);
                }
                return null;
            }

            /// <summary>
            /// 获取操作系统
            /// </summary>
            /// <returns></returns>
            public static string OperatingSystem()
            {
                string os = null;
                try
                {
                    if (File.Exists("/etc/os-release"))
                    {
                        var list = FileTo.ReadText("/etc/os-release").Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        os = list.FirstOrDefault(x => x.Contains("PRETTY_NAME")).Split('"')[1];
                    }
                    else if (File.Exists("/etc/redhat-release"))
                    {
                        os = FileTo.ReadText("/etc/redhat-release");
                    }
                    else if (File.Exists("/etc/lsb-release"))
                    {
                        var list = FileTo.ReadText("/etc/lsb-release").Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        os = list.FirstOrDefault(x => x.Contains("DESCRIPTION")).Split('"')[1];
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                return os.Trim(Environment.NewLine.ToCharArray());
            }
        }

        /// <summary>
        /// 可视化输出
        /// </summary>
        /// <returns></returns>
        public string ToView()
        {
            var dic = new Dictionary<int, string>
            {
                { 0, "" },
                { 1, $" Framework: {FrameworkDescription}" },
                { 2, $" Use Physical Memory: {ParsingTo.FormatByteSize(UsePhysicalMemory)}" },
                { 3, $" System: {(string.IsNullOrWhiteSpace(OperatingSystem) ? OS : OperatingSystem)}{(Is64BitOperatingSystem ? " , 64Bit" : "")}" },
                { 4, $" OSVersion: {OSVersion.VersionString}" },
                { 5, $" User: {UserName}" },
                { 6, $" Boot: {Math.Round(TickCount*1.0/1000/24/3600,2)} Days" },
                { 7, $" CPU: {ProcessorName} , {ProcessorCount} Core{ProgressBar(Convert.ToInt64(ProcessorUsage*100), 10000, false)}" },
                { 8, $" RAM: {ProgressBar(TotalPhysicalMemory-FreePhysicalMemory,TotalPhysicalMemory)}" }
            };
            if (SwapTotal > 0)
            {
                dic.Add(9, $" Swap: {ProgressBar(SwapTotal - SwapFree, SwapTotal)}");
            }

            var lgds = LogicalDisk as List<object>;
            var listlgd = new List<string>();
            for (int i = 0; i < lgds.Count; i++)
            {
                var lgdi = lgds[i];
                var gt = lgdi.GetType();
                var fs = Convert.ToInt64(gt.GetProperty("FreeSpace").GetValue(lgdi));
                var size = Convert.ToInt64(gt.GetProperty("Size").GetValue(lgdi));
                var name = gt.GetProperty("Name").GetValue(lgdi).ToString();
                listlgd.Add(ProgressBar(size - fs, size, true, name));
            }
            dic.Add(10, $" Disk: {string.Join(" ", listlgd)}");

            //排序
            var list = dic.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value).Values.ToList();

            return string.Join("\r\n\r\n", list);
        }

        /// <summary>
        /// 进度条
        /// </summary>
        /// <param name="m">分子</param>
        /// <param name="d">分母</param>
        /// <param name="isc">是否转换</param>
        /// <param name="desc">说明</param>
        /// <returns></returns>
        private static string ProgressBar(long m, long d, bool isc = true, string desc = "")
        {
            var vt = 24;
            var v1 = m * 1.0;
            var v2 = d * 1.0;
            var v3 = Math.Round(vt * (v1 / v2));
            var unit = string.Empty;
            if (isc)
            {
                v1 = Math.Round(m * 1.0 / 1024 / 1024 / 1024, 2);
                v2 = Math.Round(d * 1.0 / 1024 / 1024 / 1024, 2);
                unit = $" {v1}/{v2}GiB ";
            }

            if (!string.IsNullOrWhiteSpace(desc) && desc.Length > 20)
            {
                desc = desc.Substring(0, 20) + "...";
            }

            var listpb = new List<string>()
            {
                "\r\n\r\n "
            };
            while (--v3 > 0)
            {
                listpb.Add("#");
            }
            while (listpb.Count < vt)
            {
                listpb.Add(".");
            }
            var text = unit + desc;
            if (!string.IsNullOrEmpty(text))
            {
                text = $"\n{text}";
            }
            var per = " " + (v2 == 0 ? 0 : Math.Round((v1 / v2) * 100, 0)) + "%";
            text = per + text;
            listpb.Add(text);

            return string.Join("", listpb);
        }
    }
}