using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Netnr.Data
{
    public partial class ContextBase : DbContext
    {
        /// <summary>
        /// 数据库
        /// </summary>
        public enum TypeDB
        {
            MySQL,
            SQLite,
            InMemory,
            SQLServer,
            PostgreSQL
        }

        /// <summary>
        /// 数据库
        /// </summary>
        public readonly TypeDB TDB;

        public ContextBase()
        {
            Enum.TryParse(GlobalTo.GetValue("TypeDB"), true, out TDB);
        }

        public ContextBase(DbContextOptions<ContextBase> options) : base(options)
        {

        }

        private static ILoggerFactory _loggerFactory = null;
        public static ILoggerFactory LoggerFactory
        {
            get
            {
                if (_loggerFactory == null)
                {
                    var sc = new ServiceCollection();
                    sc.AddLogging(builder => builder.AddConsole().AddFilter(level => level >= LogLevel.Warning));
                    _loggerFactory = sc.BuildServiceProvider().GetService<ILoggerFactory>();
                }
                return _loggerFactory;
            }
        }

        /// <summary>
        /// 配置连接字符串
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                switch (TDB)
                {
                    case TypeDB.MySQL:
                        optionsBuilder.UseMySql(GlobalTo.GetValue("ConnectionStrings:MySQLConn"));
                        break;
                    case TypeDB.SQLite:
                        optionsBuilder.UseSqlite(GlobalTo.GetValue("ConnectionStrings:SQLiteConn").Replace("~", GlobalTo.ContentRootPath));
                        break;
                    case TypeDB.InMemory:
                        optionsBuilder.UseInMemoryDatabase(GlobalTo.GetValue("ConnectionStrings:InMemoryConn"));
                        break;
                    case TypeDB.SQLServer:
                        optionsBuilder.UseSqlServer(GlobalTo.GetValue("ConnectionStrings:SQLServerConn"), options =>
                        {
                            //启用 row_number 分页 （兼容2005、2008）
                            //options.UseRowNumberForPaging();
                        });
                        break;
                    case TypeDB.PostgreSQL:
                        optionsBuilder.UseNpgsql(GlobalTo.GetValue("ConnectionStrings:PostgreSQLConn"));
                        break;
                }

                //注册日志（修改日志等级为Information，可查看执行的SQL语句）
                optionsBuilder.UseLoggerFactory(LoggerFactory);
            }
        }
    }
}