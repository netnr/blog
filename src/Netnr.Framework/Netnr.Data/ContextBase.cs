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
            SQLServer,
            PostgreSQL
        }

        /// <summary>
        /// 数据库
        /// </summary>
        private readonly TypeDB TDB;

        /// <summary>
        /// 上下文
        /// </summary>
        public ContextBase() : base()
        {
            TDB = (TypeDB)Enum.Parse(typeof(TypeDB), GlobalTo.GetValue("TypeDB"), true);
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
            switch (TDB)
            {
                case TypeDB.SQLServer:
                    optionsBuilder.UseSqlServer(GlobalTo.GetValue("ConnectionStrings:SQLServerConn"), options =>
                    {
                        //启用 row_number 分页 （兼容2005、2008）
                        //options.UseRowNumberForPaging();
                    });
                    break;
                case TypeDB.MySQL:
                    //optionsBuilder.UseMySql(GlobalVar.GetValue("ConnectionStrings:MySQLConn"));
                    break;
                case TypeDB.SQLite:
                    //optionsBuilder.UseSqlite(GlobalVar.GetValue("ConnectionStrings:SQLiteConn"));
                    break;
                case TypeDB.PostgreSQL:
                    //optionsBuilder.UseNpgsql(GlobalVar.GetValue("ConnectionStrings:PostgreSQLConn"));
                    break;
            }

            //注册日志（修改日志等级为Information，可查看执行的SQL语句）
            optionsBuilder.UseLoggerFactory(LoggerFactory);
        }
    }
}