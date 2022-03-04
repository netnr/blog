using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Netnr.Core
{
    /// <summary>
    /// 调用cmd
    /// </summary>
    public class CmdTo
    {
        /// <summary>
        /// 是Windows系统
        /// </summary>
        public static bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        /// <summary>
        /// ProcessStartInfo
        /// </summary>
        /// <param name="Arguments">参数命令</param>
        /// <param name="FileName">执行程序，Windows默认cmd，Linux默认bash</param>
        /// <returns></returns>
        public static ProcessStartInfo PSInfo(string Arguments, string FileName = null)
        {
            var psi = new ProcessStartInfo
            {
                RedirectStandardOutput = true,  //由调用程序获取输出信息
                RedirectStandardError = true,   //重定向标准错误输出
                UseShellExecute = false,        //是否使用操作系统shell启动
                CreateNoWindow = true          //不显示程序窗口
            };

            if (string.IsNullOrWhiteSpace(FileName))
            {
                psi.FileName = IsWindows ? "cmd.exe" : "bash";
                psi.Arguments = IsWindows ? $"/C \"{Arguments}\"" : $"-c \"{Arguments}\"";
            }
            else
            {
                psi.FileName = FileName;
                psi.Arguments = Arguments;
            }

            return psi;
        }

        /// <summary>
        /// 执行（简单）
        /// </summary>
        /// <param name="Arguments">参数命令</param>
        /// <param name="FileName">执行程序，Windows默认cmd，Linux默认bash</param>
        /// <returns></returns>
        public static CliResult Execute(string Arguments, string FileName = null)
        {
            return Execute(PSInfo(Arguments, FileName), (process, cr) =>
            {
                process.Start();

                cr.CrOutput = process.StandardOutput.ReadToEnd();
                cr.CrError = process.StandardError.ReadToEnd();

                process.WaitForExit();
                process.Close();
            });
        }

        /// <summary>
        /// 执行（自定义）
        /// </summary>
        /// <param name="Arguments">参数命令</param>
        /// <param name="FileName">执行程序，Windows默认cmd，Linux默认bash</param>
        /// <param name="apc"></param>
        /// <returns></returns>
        public static CliResult Execute(string Arguments, string FileName, Action<Process, CliResult> apc)
        {
            return Execute(PSInfo(Arguments, FileName), apc);
        }

        private static CliResult Execute(Func<CliResult> action)
        {
            return action();
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="psi"></param>
        /// <param name="apc"></param>
        /// <returns></returns>
        public static CliResult Execute(ProcessStartInfo psi, Action<Process, CliResult> apc)
        {
            return Execute(() =>
            {
                var cr = new CliResult();

                using var process = new Process { StartInfo = psi };
                cr.CrProcess = process;

                //回调
                apc?.Invoke(process, cr);

                return cr;
            });
        }

        /// <summary>
        /// 输出
        /// </summary>
        public class CliResult
        {
            /// <summary>
            /// 进程
            /// </summary>
            public Process CrProcess { get; set; }

            /// <summary>
            /// 标准输出
            /// </summary>
            public string CrOutput { get; set; }
            /// <summary>
            /// 错误输出
            /// </summary>
            public string CrError { get; set; }
        }
    }
}