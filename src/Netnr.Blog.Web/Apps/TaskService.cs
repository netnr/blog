using FluentScheduler;
using Netnr.Core;

namespace Netnr.Blog.Web.Apps
{
    /// <summary>
    /// 定时任务
    /// </summary>
    public class TaskService
    {
        /// <summary>
        /// 任务注册
        /// </summary>
        public class Reg : Registry
        {
            /// <summary>
            /// 构造
            /// </summary>
            public Reg()
            {
                //Gist 同步任务
                Schedule<GistSyncJob>().ToRunEvery(8).Hours();

                //处理操作记录
                Schedule<HandleOperationRecordJob>().ToRunEvery(6).Hours();

                //数据库备份到 Git
                Schedule<DatabaseBackupToGitJob>().ToRunEvery(7).Days().At(16, 16);
            }
        }

        /// <summary>
        /// Gist同步任务
        /// </summary>
        public class GistSyncJob : IJob
        {
            void IJob.Execute()
            {
                var vm = new Controllers.ServicesController().GistSync();
                ConsoleTo.Log(vm.ToJson(true));
                Console.WriteLine(vm);
            }
        }

        /// <summary>
        /// 处理操作记录
        /// </summary>
        public class HandleOperationRecordJob : IJob
        {
            void IJob.Execute()
            {
                var vm = new Controllers.ServicesController().HandleOperationRecord();
                ConsoleTo.Log(vm.ToJson(true));
                Console.WriteLine(vm);
            }
        }

        /// <summary>
        /// 数据库备份到Git
        /// </summary>
        public class DatabaseBackupToGitJob : IJob
        {
            void IJob.Execute()
            {
                var vm = new Controllers.ServicesController().DatabaseBackupToGit();
                ConsoleTo.Log(vm.ToJson(true));
                Console.WriteLine(vm);
            }
        }
    }
}
