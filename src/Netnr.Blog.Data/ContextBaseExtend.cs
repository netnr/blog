using Microsoft.EntityFrameworkCore;
using Netnr.SharedFast;

namespace Netnr.Blog.Data
{
    /// <summary>
    /// 扩展
    /// </summary>
    public partial class ContextBase : DbContext
    {
        /// <summary>
        /// 只读（1分钟后生效，为初始化数据预留时间）
        /// </summary>
        public static void IsReadOnly()
        {
            if (GlobalTo.GetValue<bool>("ReadOnly") && Process.GetCurrentProcess().StartTime.AddMinutes(1) < DateTime.Now)
            {
                throw new Exception("The database is read-only");
            }
        }

        public override int SaveChanges()
        {
            IsReadOnly();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            IsReadOnly();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            IsReadOnly();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            IsReadOnly();
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}