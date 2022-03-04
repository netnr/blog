#if Full || Public

namespace Netnr
{
    /// <summary>
    /// 共享枚举
    /// </summary>
    public class SharedEnum
    {
        /// <summary>
        /// 数据库类型
        /// </summary>
        public enum TypeDB
        {
            /// <summary>
            /// Memory
            /// </summary>
            InMemory,
            /// <summary>
            /// SQLite
            /// </summary>
            SQLite,
            /// <summary>
            /// MySQL
            /// </summary>
            MySQL,
            /// <summary>
            /// MariaDB
            /// </summary>
            MariaDB,
            /// <summary>
            /// Oracle
            /// </summary>
            Oracle,
            /// <summary>
            /// SQLServer
            /// </summary>
            SQLServer,
            /// <summary>
            /// PostgreSQL
            /// </summary>
            PostgreSQL
        }

        /// <summary>
        /// 返回结果常用类型
        /// </summary>
        public enum RTag
        {
            /// <summary>
            /// 成功
            /// </summary>
            success = 200,
            /// <summary>
            /// 失败
            /// </summary>
            fail = 400,
            /// <summary>
            /// 错误
            /// </summary>
            error = 500,
            /// <summary>
            /// 未授权
            /// </summary>
            unauthorized = 401,
            /// <summary>
            /// 拒绝
            /// </summary>
            refuse = 403,
            /// <summary>
            /// 存在
            /// </summary>
            exist = 97,
            /// <summary>
            /// 无效
            /// </summary>
            invalid = 95,
            /// <summary>
            /// 缺省
            /// </summary>
            lack = 410,
            /// <summary>
            /// 异常
            /// </summary>
            exception = -1
        }
    }
}

#endif