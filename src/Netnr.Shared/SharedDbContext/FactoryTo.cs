# if Full || DbContext

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Netnr.SharedFast;

namespace Netnr.SharedDbContext
{
    /// <summary>
    /// 数据库工厂
    /// </summary>
    public class FactoryTo
    {
        /// <summary>
        /// 应用程序不为每个上下文实例创建新的ILoggerFactory实例非常重要。这样做会导致内存泄漏和性能下降
        /// </summary>
        private static ILoggerFactory logFactory = null;
        public static ILoggerFactory LogFactory
        {
            get
            {
                if (logFactory == null)
                {
                    logFactory = LoggerFactory.Create(logging => logging.AddConsole().AddFilter(level => level >= LogLevel.Information));
                }
                return logFactory;
            }
        }

        /// <summary>
        /// 创建 DbContextOptionsBuilder
        /// </summary>
        /// <param name="tdb">数据库类型</param>
        /// <param name="connnectionString">连接字符串</param>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static DbContextOptionsBuilder<T> CreateDbContextOptionsBuilder<T>(SharedEnum.TypeDB tdb, string connnectionString, DbContextOptionsBuilder builder = null) where T : DbContext
        {
            if (builder == null)
            {
                builder = new DbContextOptionsBuilder<T>();
            }

            if (!builder.IsConfigured)
            {
                switch (tdb)
                {
                    case SharedEnum.TypeDB.InMemory:
#if DbContextInMemory
                        builder.UseInMemoryDatabase(connnectionString);
#endif
                        break;
                    case SharedEnum.TypeDB.SQLite:
#if DbContextSQLite
                        builder.UseSqlite(connnectionString);
#endif
                        break;
                    case SharedEnum.TypeDB.MySQL:
                    case SharedEnum.TypeDB.MariaDB:
#if DbContextMySQL
                        builder.UseMySql(connnectionString, ServerVersion.AutoDetect(connnectionString));
#endif
                        break;
                    case SharedEnum.TypeDB.Oracle:
#if DbContextOracle
                        builder.UseOracle(connnectionString);
#endif
                        break;
                    case SharedEnum.TypeDB.SQLServer:
#if DbContextSQLServer
                        builder.UseSqlServer(connnectionString);
#endif
                        break;
                    case SharedEnum.TypeDB.PostgreSQL:
#if DbContextPostgreSQL
                        builder.UseNpgsql(connnectionString);
#endif
                        break;
                }
            }
            builder.UseLoggerFactory(LogFactory).EnableSensitiveDataLogging().EnableDetailedErrors();

            return builder as DbContextOptionsBuilder<T>;
        }

        /// <summary>
        /// 获取连接字符串
        /// </summary>
        /// <param name="typeDB"></param>
        /// <returns></returns>
        public static string GetConn(SharedEnum.TypeDB? typeDB = null)
        {
            var tdb = typeDB ?? GlobalTo.TDB;
            var conn = GlobalTo.Configuration.GetConnectionString(tdb.ToString());
            if (tdb != SharedEnum.TypeDB.InMemory)
            {
                //环境变量，如：$DATABASE_URL
                if (conn.StartsWith("$"))
                {
                    conn = SharedAdo.DbHelper.SqlConnFromHeroku(conn[1..]);
                }
                else
                {
                    var pwd = GlobalTo.GetValue("ConnectionStrings:Password");
                    conn = SharedAdo.DbHelper.SqlConnEncryptOrDecrypt(conn, pwd);

                    if (tdb == SharedEnum.TypeDB.SQLite)
                    {
                        conn = conn.Replace("~", GlobalTo.ContentRootPath);
                    }

                    conn = SharedAdo.DbHelper.SqlConnPreCheck(tdb, conn);
                }

                return conn;
            }
            return null;
        }
    }
}

#endif