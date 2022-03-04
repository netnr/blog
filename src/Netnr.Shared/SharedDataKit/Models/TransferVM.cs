#if Full || DataKit

using Netnr.SharedAdo;

namespace Netnr.SharedDataKit
{
    /// <summary>
    /// 传递参数
    /// </summary>
    public partial class TransferVM
    {
        /// <summary>
        /// 连接对象
        /// </summary>
        public class ConnectionInfo
        {
            /// <summary>
            /// 连接类型
            /// </summary>
            public SharedEnum.TypeDB ConnectionType { get; set; }
            private string _connectionString;
            /// <summary>
            /// 连接字符串
            /// </summary>
            public string ConnectionString
            {
                get
                {
                    var conn = DbHelper.SqlConnPreCheck(ConnectionType, _connectionString);
                    return conn;
                }
                set
                {
                    _connectionString = value;
                }
            }
            /// <summary>
            /// 连接备注
            /// </summary>
            public string ConnectionRemark { get; set; }
            private string _DatabaseName = null;
            /// <summary>
            /// 数据库名（未设置时获取默认数据库名）
            /// </summary>
            public string DatabaseName
            {
                get
                {
                    if (string.IsNullOrWhiteSpace(_DatabaseName))
                    {
                        //默认数据库名
                        return DataKit.DbConn(ConnectionType, ConnectionString).Database;
                    }
                    else
                    {
                        return _DatabaseName;
                    }
                }
                set
                {
                    ConnectionString = DataKit.SetConnDatabase(ConnectionType, ConnectionString, value);
                    _DatabaseName = value;
                }
            }

            /// <summary>
            /// 新实例
            /// </summary>
            /// <returns></returns>
            public DbHelper NewDbHelper()
            {
                var db = new DbHelper(DataKit.DbConn(ConnectionType, ConnectionString));
                return db;
            }
        }

        /// <summary>
        /// 读写项
        /// </summary>
        public class ReadWriteItem
        {
            /// <summary>
            /// 读取表数据SQL（可带模式名 SchemaName）
            /// </summary>
            public string ReadDataSQL { get; set; }
            /// <summary>
            /// 读取表名（可带模式名 SchemaName，用于生成列映射）
            /// </summary>
            public string ReadTableName { get; set; }
            /// <summary>
            /// 写入表名（可带模式名 SchemaName）
            /// </summary>
            public string WriteTableName { get; set; }
            /// <summary>
            /// 清空写入表SQL（可带模式名 SchemaName）
            /// </summary>
            public string WriteDeleteSQL { get; set; }
            /// <summary>
            /// 读取列 映射 写入列
            /// </summary>
            public Dictionary<string, string> ReadWriteColumnMap { get; set; } = new Dictionary<string, string>();
        }

        /// <summary>
        /// 迁移
        /// </summary>
        public class MigrateBase
        {
            /// <summary>
            /// 读取连接信息
            /// </summary>
            public ConnectionInfo ReadConnectionInfo { get; set; }
            /// <summary>
            /// 读取连接信息（引用）
            /// </summary>
            public string RefReadConnectionInfo { get; set; }
            /// <summary>
            /// 读取数据库名（读取配置，还需回填 WriteConnectionInfo）
            /// </summary>
            public string ReadDatabaseName { get; set; }
            /// <summary>
            /// 写入连接信息
            /// </summary>
            public ConnectionInfo WriteConnectionInfo { get; set; }
            /// <summary>
            /// 写入连接信息（引用）
            /// </summary>
            public string RefWriteConnectionInfo { get; set; }
            /// <summary>
            /// 写入数据库名（读取配置，还需回填 WriteConnectionInfo）
            /// </summary>
            public string WriteDatabaseName { get; set; }
        }

        /// <summary>
        /// 迁移数据表
        /// </summary>
        public class MigrateDataTable : MigrateBase
        {
            /// <summary>
            /// 读写表集合
            /// </summary>
            public List<ReadWriteItem> ListReadWrite { get; set; } = new List<ReadWriteItem>();
        }

        /// <summary>
        /// 迁移数据库
        /// </summary>
        public class MigrateDatabase : MigrateBase
        {
            /// <summary>
            /// 写入前删除表数据
            /// </summary>
            public bool WriteDeleteData { get; set; }
            /// <summary>
            /// 转换为
            /// </summary>
            /// <returns></returns>
            public MigrateDataTable AsMigrateDataTable()
            {
                var mdb = this;
                var mdt = new MigrateDataTable().ToRead(mdb);

                var readTables = DataKit.Init(mdt.ReadConnectionInfo).GetTable();
                var writeTables = DataKit.Init(mdt.WriteConnectionInfo).GetTable();

                if (readTables?.Count > 0 && writeTables?.Count > 0)
                {
                    readTables.ForEach(readTable =>
                    {
                        //读取库的表名 在 写入库
                        var listWriteTable = writeTables.Where(x => readTable.TableName == x.TableName).ToList();
                        if (listWriteTable.Count > 0)
                        {
                            //尝试匹配模式名 或 取第一条
                            var writeTable = listWriteTable.FirstOrDefault(x => x.SchemaName == readTable.SchemaName);
                            if (writeTable == null)
                            {
                                writeTable = listWriteTable.First();
                            }

                            var readSNTN = DbHelper.SqlSNTN(readTable.TableName, readTable.SchemaName, mdt.ReadConnectionInfo.ConnectionType);
                            var writeSNTN = DbHelper.SqlSNTN(writeTable.TableName, writeTable.SchemaName, mdt.WriteConnectionInfo.ConnectionType);

                            var clearTableSql = $"{(mdt.WriteConnectionInfo.ConnectionType == SharedEnum.TypeDB.SQLite ? "DELETE FROM" : "TRUNCATE TABLE")} {writeSNTN}";

                            mdt.ListReadWrite.Add(new ReadWriteItem
                            {
                                ReadDataSQL = $"SELECT * FROM {readSNTN}",
                                WriteTableName = DbHelper.SqlSNTN(writeTable.TableName, writeTable.SchemaName),
                                WriteDeleteSQL = mdb.WriteDeleteData ? clearTableSql : null
                            });
                        }
                    });
                }

                return mdt;
            }
        }

        /// <summary>
        /// 导出
        /// </summary>
        public class ExportBase
        {
            /// <summary>
            /// 连接信息
            /// </summary>
            public ConnectionInfo ReadConnectionInfo { get; set; }
            /// <summary>
            /// 读取连接信息（引用）
            /// </summary>
            public string RefReadConnectionInfo { get; set; }
            /// <summary>
            /// 读取数据库名（读取配置，还需回填 WriteConnectionInfo）
            /// </summary>
            public string ReadDatabaseName { get; set; }
            /// <summary>
            /// 读取模式名
            /// </summary>
            public List<string> ListReadSchemaName { get; set; } = new List<string>();
            /// <summary>
            /// 导出 ZIP 包完整路径
            /// </summary>
            public string ZipPath { get; set; }
        }

        /// <summary>
        /// 导出表
        /// </summary>
        public class ExportDataTable : ExportBase
        {
            /// <summary>
            /// 读取数据表（可带模式名 SchemaName）
            /// </summary>
            public List<string> ListReadDataSQL { get; set; } = new List<string>();
        }

        /// <summary>
        /// 导出库
        /// </summary>
        public class ExportDatabase : ExportBase
        {
            /// <summary>
            /// 读取表名（可带模式名 SchemaName）
            /// </summary>
            public List<string> ListReadTableName { get; set; } = new List<string>();
        }

        /// <summary>
        /// 导入数据库
        /// </summary>
        public class ImportDatabase
        {
            /// <summary>
            /// 连接信息
            /// </summary>
            public ConnectionInfo WriteConnectionInfo { get; set; }
            /// <summary>
            /// 写入连接信息（引用）
            /// </summary>
            public string RefWriteConnectionInfo { get; set; }
            /// <summary>
            /// 写入数据库名（读取配置，还需回填 WriteConnectionInfo）
            /// </summary>
            public string WriteDatabaseName { get; set; }
            /// <summary>
            /// 导入 ZIP 包完整路径
            /// </summary>
            public string ZipPath { get; set; }
            /// <summary>
            /// 写入前删除表数据
            /// </summary>
            public bool WriteDeleteData { get; set; }
            /// <summary>
            /// 读取表 映射 写入表（可带模式名 SchemaName）
            /// </summary>
            public Dictionary<string, string> ReadWriteTableMap { get; set; } = new Dictionary<string, string>();
        }
    }
}

#endif