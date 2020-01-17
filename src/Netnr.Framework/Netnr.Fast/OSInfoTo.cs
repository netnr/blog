using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;

namespace Netnr.Fast
{
    /// <summary>
    /// 系统信息
    /// </summary>
    public class OSInfoTo
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
        /// 逻辑磁盘
        /// </summary>
        public object LogicalDisk { get; set; }

        /// <summary>
        /// 构造
        /// </summary>
        public OSInfoTo()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                OS = OSPlatform.Windows.ToString();

                TotalPhysicalMemory = PlatformForWindows.TotalPhysicalMemory();
                FreePhysicalMemory = PlatformForWindows.FreePhysicalMemory();

                LogicalDisk = PlatformForWindows.LogicalDisk();

                ProcessorName = PlatformForWindows.ProcessorName();

                TickCount = PlatformForWindows.RunTime();
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                OS = OSPlatform.Linux.ToString();

                TotalPhysicalMemory = PlatformForLinux.MemInfo("MemTotal:");
                FreePhysicalMemory = PlatformForLinux.MemInfo("MemAvailable:");

                SwapFree = PlatformForLinux.MemInfo("SwapFree:");
                SwapTotal = PlatformForLinux.MemInfo("SwapTotal:");

                LogicalDisk = PlatformForLinux.LogicalDisk();

                ProcessorName = PlatformForLinux.CpuInfo("model name");

                TickCount = PlatformForLinux.RunTime();
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                OS = OSPlatform.OSX.ToString();

                TotalPhysicalMemory = PlatformForLinux.MemInfo("MemTotal:");
                FreePhysicalMemory = PlatformForLinux.MemInfo("MemAvailable:");

                LogicalDisk = PlatformForLinux.LogicalDisk();

                ProcessorName = PlatformForLinux.CpuInfo("model name");

                TickCount = PlatformForLinux.RunTime();
            }

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
                long TotalPhysicalMemory = 0;

                using var mc = new ManagementClass("Win32_ComputerSystem");
                var moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if (mo["TotalPhysicalMemory"] != null)
                    {
                        TotalPhysicalMemory = long.Parse(mo["TotalPhysicalMemory"].ToString());

                        break;
                    }
                }

                return TotalPhysicalMemory;
            }

            /// <summary>
            /// 获取可用内存 B
            /// </summary>
            public static long FreePhysicalMemory()
            {
                long FreePhysicalMemory = 0;

                using var mos = new ManagementClass("Win32_OperatingSystem");
                foreach (ManagementObject mo in mos.GetInstances())
                {
                    if (mo["FreePhysicalMemory"] != null)
                    {
                        FreePhysicalMemory = 1024 * long.Parse(mo["FreePhysicalMemory"].ToString());

                        break;
                    }
                }

                return FreePhysicalMemory;
            }

            /// <summary>
            /// 获取磁盘信息
            /// </summary>
            /// <returns></returns>
            public static List<object> LogicalDisk()
            {
                var listld = new List<object>();

                using var diskClass = new ManagementClass("Win32_LogicalDisk");
                var disks = diskClass.GetInstances();
                foreach (ManagementObject disk in disks)
                {
                    // DriveType.Fixed 为固定磁盘(硬盘) 
                    if (int.Parse(disk["DriveType"].ToString()) == (int)DriveType.Fixed)
                    {
                        listld.Add(new
                        {
                            Name = disk["Name"],
                            Size = disk["Size"],
                            FreeSpace = disk["FreeSpace"]
                        });
                    }
                }

                return listld;
            }

            /// <summary>
            /// 获取处理器名称
            /// </summary>
            /// <returns></returns>
            public static string ProcessorName()
            {
                var cmd = "wmic cpu get name";
                var cr = Core.CmdTo.Run(cmd).TrimEnd(Environment.NewLine.ToCharArray());
                var pvalue = cr.Split(Environment.NewLine.ToCharArray()).LastOrDefault();
                return pvalue;
            }

            /// <summary>
            /// 运行时长
            /// </summary>
            /// <returns></returns>
            public static long RunTime()
            {
                var cmd = "net statistics WORKSTATION";
                var cr = Core.CmdTo.Run(cmd).Split(Environment.NewLine.ToCharArray())[14].Split(' ').ToList();
                while (!"1234567890".Contains(cr.First()[0]))
                {
                    cr.RemoveAt(0);
                }
                DateTime.TryParse(string.Join(" ", cr), out DateTime startTime);
                var pvalue = Convert.ToInt64((DateTime.Now - startTime).TotalMilliseconds);

                return pvalue;
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
                var meminfo = Core.FileTo.ReadText("/proc/", "meminfo");
                var pitem = meminfo.Split(Environment.NewLine.ToCharArray()).FirstOrDefault(x => x.StartsWith(pkey));

                var pvalue = 1024 * long.Parse(pitem.Replace(pkey, "").ToLower().Replace("kb", "").Trim());

                return pvalue;
            }

            /// <summary>
            /// 获取 /proc/cpuinfo
            /// </summary>
            /// <param name="pkey"></param>
            /// <returns></returns>
            public static string CpuInfo(string pkey)
            {
                var meminfo = Core.FileTo.ReadText("/proc/", "cpuinfo");
                var pitem = meminfo.Split(Environment.NewLine.ToCharArray()).FirstOrDefault(x => x.StartsWith(pkey));

                var pvalue = pitem.Split(':')[1].Trim();

                return pvalue;
            }

            /// <summary>
            /// 获取磁盘信息
            /// </summary>
            /// <returns></returns>
            public static List<object> LogicalDisk()
            {
                var listld = new List<object>();

                var dfresult = Core.CmdTo.Shell("df");
                var listdev = dfresult.Output.Split(Environment.NewLine.ToCharArray()).Where(x => x.StartsWith("/dev/"));
                foreach (var devitem in listdev)
                {
                    var dis = devitem.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                    listld.Add(new
                    {
                        Name = dis[0],
                        Size = long.Parse(dis[1]) * 1024,
                        FreeSpace = long.Parse(dis[3]) * 1024
                    });
                }

                return listld;
            }

            /// <summary>
            /// 获取CPU使用率 %
            /// </summary>
            /// <returns></returns>
            public static float CPULoad()
            {
                var br = Core.CmdTo.Shell("vmstat 1 2");
                var cpuitems = br.Output.Split(Environment.NewLine.ToCharArray()).LastOrDefault().Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                var us = cpuitems[cpuitems.Count - 5];

                return float.Parse(us);
            }

            /// <summary>
            /// 运行时长
            /// </summary>
            /// <returns></returns>
            public static long RunTime()
            {
                var uptime = Core.FileTo.ReadText("/proc/", "uptime");
                var pitem = Convert.ToDouble(uptime.Split(' ')[0]);

                var pvalue = Convert.ToInt64(pitem * 1000); ;

                return pvalue;
            }
        }
    }
}