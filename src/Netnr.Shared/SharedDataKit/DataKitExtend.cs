#if Full || DataKit

using System.IO.Compression;
using Microsoft.Data.Sqlite;
using MySqlConnector;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Data.SqlClient;
using Npgsql;
using Netnr.Core;
using Netnr.SharedAdo;

namespace Netnr.SharedDataKit
{
    /// <summary>
    /// DataKit 拓展
    /// </summary>
    public partial class DataKit
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="tdb"></param>
        /// <param name="conn"></param>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        public static DataKit Init(SharedEnum.TypeDB tdb, string conn, string databaseName = null)
        {
            DataKit dk = null;

            try
            {
                //连接信息
                DbConnection dbConnection = null;
                //打印信息
                var listInfo = new List<string>();

                //额外处理 SQLite
                if (tdb == SharedEnum.TypeDB.SQLite)
                {
                    //下载 SQLite 文件
                    var ds = conn[12..].TrimEnd(';');
                    //路径
                    var dspath = Path.GetTempPath();
                    //文件名
                    var dsname = Path.GetFileName(ds);
                    var fullPath = Path.Combine(dspath, dsname);

                    //网络路径
                    if (ds.ToLower().StartsWith("http"))
                    {
                        //不存在则下载
                        if (!File.Exists(fullPath))
                        {
                            //下载
                            HttpTo.DownloadSave(HttpTo.HWRequest(ds), fullPath);
                        }

                        conn = "Data Source=" + fullPath;
                    }
                    else
                    {
                        conn = "Data Source=" + ds;
                    }
                }

                conn = DbHelper.SqlConnPreCheck(tdb, conn);
                switch (tdb)
                {
                    case SharedEnum.TypeDB.SQLite:
                        dbConnection = new SqliteConnection(conn);
                        break;
                    case SharedEnum.TypeDB.MySQL:
                    case SharedEnum.TypeDB.MariaDB:
                        {
                            var csb = new MySqlConnectionStringBuilder(conn);
                            if (!string.IsNullOrWhiteSpace(databaseName))
                            {
                                csb.Database = databaseName;
                            }
                            dbConnection = new MySqlConnection(csb.ConnectionString);
                        }
                        break;
                    case SharedEnum.TypeDB.Oracle:
                        {
                            dbConnection = new OracleConnection(conn);
                        }
                        break;
                    case SharedEnum.TypeDB.SQLServer:
                        {
                            var csb = new SqlConnectionStringBuilder(conn);
                            if (!string.IsNullOrWhiteSpace(databaseName))
                            {
                                csb.InitialCatalog = databaseName;
                            }
                            dbConnection = new SqlConnection(csb.ConnectionString);
                        }
                        break;
                    case SharedEnum.TypeDB.PostgreSQL:
                        {
                            var csb = new NpgsqlConnectionStringBuilder(conn);
                            if (!string.IsNullOrWhiteSpace(databaseName))
                            {
                                csb.Database = databaseName;
                            }
                            dbConnection = new NpgsqlConnection(csb.ConnectionString);
                        }
                        break;
                }

                dk = new DataKit(tdb, dbConnection);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return dk;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="connectionInfo">连接信息</param>
        /// <returns></returns>
        public static DataKit Init(TransferVM.ConnectionInfo connectionInfo)
        {
            DataKit dk = Init(connectionInfo.ConnectionType, connectionInfo.ConnectionString, connectionInfo.DatabaseName);

            return dk;
        }

        /// <summary>
        /// 数据库连接
        /// </summary>
        /// <param name="tdb"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static DbConnection DbConn(SharedEnum.TypeDB tdb, string conn)
        {
            return tdb switch
            {
                SharedEnum.TypeDB.SQLite => new SqliteConnection(conn),
                SharedEnum.TypeDB.MySQL or SharedEnum.TypeDB.MariaDB => new MySqlConnection(conn),
                SharedEnum.TypeDB.Oracle => new OracleConnection(conn),
                SharedEnum.TypeDB.SQLServer => new SqlConnection(conn),
                SharedEnum.TypeDB.PostgreSQL => new NpgsqlConnection(conn),
                _ => null,
            };
        }

        /// <summary>
        /// 设置连接的数据库名（MySQL、SQLServer、PostgreSQL）
        /// </summary>
        /// <param name="tdb"></param>
        /// <param name="conn"></param>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        public static string SetConnDatabase(SharedEnum.TypeDB tdb, string conn, string databaseName)
        {
            return tdb switch
            {
                SharedEnum.TypeDB.MySQL or SharedEnum.TypeDB.MariaDB => new MySqlConnectionStringBuilder(conn)
                {
                    Database = databaseName
                }.ConnectionString,
                SharedEnum.TypeDB.SQLServer => new SqlConnectionStringBuilder(conn)
                {
                    InitialCatalog = databaseName
                }.ConnectionString,
                SharedEnum.TypeDB.PostgreSQL => new NpgsqlConnectionStringBuilder(conn)
                {
                    Database = databaseName
                }.ConnectionString,
                _ => conn,
            };
        }

        //分批最大行数
        public const int BatchMaxRows = 10000;

        /// <summary>
        /// 导出数据库
        /// </summary>
        /// <param name="edb"></param>
        /// <param name="le">日志事件</param>
        /// <returns></returns>
        public static SharedResultVM ExportDatabase(TransferVM.ExportDatabase edb, Action<NotifyCollectionChangedEventArgs> le = null)
        {
            if (edb.ListReadTableName.Count == 0)
            {
                var dk = Init(edb.ReadConnectionInfo);
                edb.ListReadTableName = dk.GetTable().Select(x => DbHelper.SqlSNTN(x.TableName, x.SchemaName, edb.ReadConnectionInfo.ConnectionType)).ToList();
            }

            var edt = new TransferVM.ExportDataTable().ToRead(edb);
            edt.ListReadDataSQL = new List<string>();

            foreach (var table in edb.ListReadTableName)
            {
                var sql = $"SELECT * FROM {table}";
                edt.ListReadDataSQL.Add(sql);
            }

            return ExportDataTable(edt, le);
        }

        /// <summary>
        /// 导出表
        /// </summary>
        /// <param name="edt"></param>
        /// <param name="le">日志事件</param>
        /// <returns></returns>
        public static SharedResultVM ExportDataTable(TransferVM.ExportDataTable edt, Action<NotifyCollectionChangedEventArgs> le = null)
        {
            var vm = new SharedResultVM();
            vm.LogEvent(le);

            vm.Log.Add($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss} 导出数据\n");

            //数据库
            var db = edt.ReadConnectionInfo.NewDbHelper();

            //打包
            var zipFolder = Path.GetDirectoryName(edt.ZipPath);
            if (!Directory.Exists(zipFolder))
            {
                Directory.CreateDirectory(zipFolder);
            }
            var isOldZip = File.Exists(edt.ZipPath);
            using ZipArchive zip = ZipFile.Open(edt.ZipPath, isOldZip ? ZipArchiveMode.Update : ZipArchiveMode.Create);

            for (int i = 0; i < edt.ListReadDataSQL.Count; i++)
            {
                var sql = edt.ListReadDataSQL[i];

                vm.Log.Add($"读取表：{sql}");

                var rowCount = 0;
                var batchNo = 0;

                var dt = new DataTable();
                //模式名.表名
                var sntn = string.Empty;

                var sw = new Stopwatch();
                sw.Start();

                //读取行
                db.SqlExecuteDataRow(sql, dr =>
                {
                    rowCount++;

                    if (rowCount == 1)
                    {
                        dt = dr.Table.Clone();
                        sntn = DbHelper.SqlSNTN(dt.TableName, dt.Namespace);
                    }

                    dt.Rows.Add(dr.ItemArray);

                    if (sw.Elapsed.TotalMilliseconds > 5000)
                    {
                        vm.Log.Add($"当前读取行：{dt.Rows.Count}/{rowCount}");
                        sw.Restart();
                    }

                    if (dt.Rows.Count >= BatchMaxRows)
                    {
                        batchNo++;

                        var xmlName = $"{sntn}_{batchNo.ToString().PadLeft(7, '0')}.xml";

                        //xml 写入 zip
                        var zae = isOldZip
                        ? zip.GetEntry(xmlName) ?? zip.CreateEntry(xmlName)
                        : zip.CreateEntry(xmlName);

                        var ceStream = zae.Open();
                        dt.WriteXml(ceStream, XmlWriteMode.WriteSchema);
                        ceStream.Close();

                        vm.Log.Add($"导出表（{dt.TableName}）第 {batchNo} 批（行：{dt.Rows.Count}/{rowCount}），耗时：{vm.PartTimeFormat()}");
                        sw.Restart();

                        dt.Clear();
                    }
                });

                if (batchNo == 0 && dt.Rows.Count == 0)
                {
                    vm.Log.Add($"跳过空表，导表进度：{i + 1}/{edt.ListReadDataSQL.Count}\n");
                }
                else
                {
                    //最后一批
                    if (dt.Rows.Count > 0)
                    {
                        batchNo++;

                        var xmlName = $"{sntn}_{batchNo.ToString().PadLeft(7, '0')}.xml";

                        //xml 写入 zip
                        var zae = isOldZip
                        ? zip.GetEntry(xmlName) ?? zip.CreateEntry(xmlName)
                        : zip.CreateEntry(xmlName);

                        var ceStream = zae.Open();
                        dt.WriteXml(ceStream, XmlWriteMode.WriteSchema);
                        ceStream.Close();

                        vm.Log.Add($"导出表（{sntn}）第 {batchNo} 批（行：{dt.Rows.Count}/{rowCount}），耗时：{vm.PartTimeFormat()}");
                    }

                    vm.Log.Add($"导出表（{sntn}）完成（行：{rowCount}），导表进度：{i + 1}/{edt.ListReadDataSQL.Count}\n");
                }

                //清理包历史分片
                if (isOldZip)
                {
                    var hasOldData = false;
                    do
                    {
                        var xmlName = $"{sntn}_{(++batchNo).ToString().PadLeft(7, '0')}.xml";
                        var zae = zip.GetEntry(xmlName);
                        if (hasOldData = (zae != null))
                        {
                            zae.Delete();
                        }
                    } while (hasOldData);
                }
            }

            vm.Log.Add($"导出完成：{edt.ZipPath}，共耗时：{vm.TotalTimeFormat()}\n");
            GC.Collect();

            vm.Set(SharedEnum.RTag.success);
            return vm;
        }

        /// <summary>
        /// 迁移数据表
        /// </summary>
        /// <param name="mdt"></param>
        /// <param name="le">实时日志</param>
        /// <returns></returns>
        public static SharedResultVM MigrateDataTable(TransferVM.MigrateDataTable mdt, Action<NotifyCollectionChangedEventArgs> le = null)
        {
            var vm = new SharedResultVM();
            vm.LogEvent(le);

            vm.Log.Add($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss} 迁移表数据\n");

            //读取数据库
            var dbRead = mdt.ReadConnectionInfo.NewDbHelper();
            //写入数据库
            var dbWrite = mdt.WriteConnectionInfo.NewDbHelper();

            //遍历
            for (int i = 0; i < mdt.ListReadWrite.Count; i++)
            {
                var rw = mdt.ListReadWrite[i];

                vm.Log.Add($"读取表（{rw.WriteTableName}）结构");
                var dtWrite = dbWrite.SqlExecuteReader(DbHelper.SqlEmpty(rw.WriteTableName, tdb: mdt.WriteConnectionInfo.ConnectionType)).Item1.Tables[0];
                dtWrite.TableName = rw.WriteTableName;
                var dtWriteColumnName = dtWrite.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList();

                //读取表的列 => 写入表的列
                var rwMap = new Dictionary<string, string>();

                vm.Log.Add($"读取表数据 SQL：{rw.ReadDataSQL}");

                var rowCount = 0;
                var batchNo = 0;
                var catchCount = 0;

                //读取行
                dbRead.SqlExecuteDataRow(rw.ReadDataSQL, drRead =>
                {
                    rowCount++;

                    //构建列映射
                    if (rowCount == 1)
                    {
                        foreach (DataColumn dcRead in drRead.Table.Columns)
                        {
                            //指定映射
                            if (rw.ReadWriteColumnMap.ContainsKey(dcRead.ColumnName))
                            {
                                rwMap.Add(dcRead.ColumnName, rw.ReadWriteColumnMap[dcRead.ColumnName]);
                            }
                            else
                            {
                                //自动映射
                                var columnNameWrite = dtWriteColumnName.FirstOrDefault(x => x.ToLower() == dcRead.ColumnName.ToLower());
                                if (columnNameWrite != null)
                                {
                                    rwMap.Add(dcRead.ColumnName, columnNameWrite);
                                }
                            }
                        }
                    }

                    //构建一行 写入表
                    var drWriteNew = dtWrite.NewRow();
                    //根据读取表列映射写入表列填充单元格数据
                    foreach (var columnNameRead in rwMap.Keys)
                    {
                        //读取表列值
                        var valueRead = drRead[columnNameRead];
                        if (valueRead is not DBNull)
                        {
                            //读取表列值 转换类型为 写入表列值
                            try
                            {
                                //写入表列
                                var columnNameWrite = rwMap[columnNameRead];
                                //写入表列类型
                                var typeWrite = dtWrite.Columns[columnNameWrite].DataType;

                                //读取表列值 赋予 写入表列
                                drWriteNew[columnNameWrite] = Convert.ChangeType(valueRead, typeWrite);
                            }
                            catch (Exception ex)
                            {
                                catchCount++;
                                vm.Log.Add($"列值转换失败：");
                                vm.Log.Add(ex.ToJson());
                            }
                        }
                    }
                    //添加新行
                    dtWrite.Rows.Add(drWriteNew.ItemArray);

                    if (dtWrite.Rows.Count >= BatchMaxRows)
                    {
                        batchNo++;

                        if (batchNo == 1)
                        {
                            //第一批写入前清理表
                            if (!string.IsNullOrWhiteSpace(rw.WriteDeleteSQL))
                            {
                                vm.Log.Add($"清理写入表：{rw.WriteDeleteSQL}");
                                var num = dbWrite.SqlExecuteReader(rw.WriteDeleteSQL).Item2;
                                vm.Log.Add($"返回受影响行数：{num}，耗时：{vm.PartTimeFormat()}");
                            }
                        }

                        //分批写入
                        vm.Log.Add($"写入表（{rw.WriteTableName}）第 {batchNo} 批（行：{dtWrite.Rows.Count}/{rowCount}）");

                        switch (mdt.WriteConnectionInfo.ConnectionType)
                        {
                            case SharedEnum.TypeDB.SQLite:
                                dbWrite.BulkBatchSQLite(dtWrite);
                                break;
                            case SharedEnum.TypeDB.MySQL:
                            case SharedEnum.TypeDB.MariaDB:
                                dbWrite.BulkCopyMySQL(dtWrite);
                                break;
                            case SharedEnum.TypeDB.Oracle:
                                dbWrite.BulkCopyOracle(dtWrite);
                                break;
                            case SharedEnum.TypeDB.SQLServer:
                                dbWrite.BulkCopySQLServer(dtWrite);
                                break;
                            case SharedEnum.TypeDB.PostgreSQL:
                                dbWrite.BulkKeepIdentityPostgreSQL(dtWrite);
                                break;
                        }

                        //清理
                        dtWrite.Clear();
                    }
                });

                //最后一批
                if (dtWrite.Rows.Count > 0)
                {
                    batchNo++;

                    if (batchNo == 1)
                    {
                        //第一批写入前清理表
                        if (!string.IsNullOrWhiteSpace(rw.WriteDeleteSQL))
                        {
                            vm.Log.Add($"清理写入表：{rw.WriteDeleteSQL}");
                            var num = dbWrite.SqlExecuteReader(rw.WriteDeleteSQL).Item2;
                            vm.Log.Add($"返回受影响行数：{num}，耗时：{vm.PartTimeFormat()}");
                        }
                    }

                    //分批写入
                    vm.Log.Add($"写入表（{rw.WriteTableName}）第 {batchNo} 批（行：{dtWrite.Rows.Count}/{rowCount}）");

                    switch (mdt.WriteConnectionInfo.ConnectionType)
                    {
                        case SharedEnum.TypeDB.SQLite:
                            dbWrite.BulkBatchSQLite(dtWrite);
                            break;
                        case SharedEnum.TypeDB.MySQL:
                        case SharedEnum.TypeDB.MariaDB:
                            dbWrite.BulkCopyMySQL(dtWrite);
                            break;
                        case SharedEnum.TypeDB.Oracle:
                            dbWrite.BulkCopyOracle(dtWrite);
                            break;
                        case SharedEnum.TypeDB.SQLServer:
                            dbWrite.BulkCopySQLServer(dtWrite);
                            break;
                        case SharedEnum.TypeDB.PostgreSQL:
                            dbWrite.BulkKeepIdentityPostgreSQL(dtWrite);
                            break;
                    }

                    //清理
                    dtWrite.Clear();
                }

                if (catchCount > 0)
                {
                    vm.Log.Add($"列值转换失败：{catchCount} 次");
                }
                vm.Log.Add($"写入表（{rw.WriteTableName}）完成，耗时：{vm.PartTimeFormat()}，写表进度：{i + 1}/{mdt.ListReadWrite.Count}\n");
            }

            vm.Log.Add($"总共耗时：{vm.TotalTimeFormat()}\n");
            GC.Collect();

            vm.Set(SharedEnum.RTag.success);
            return vm;
        }

        /// <summary>
        /// 导入数据
        /// </summary>
        /// <param name="idb"></param>
        /// <param name="le">日志事件</param>
        /// <returns></returns>
        public static SharedResultVM ImportDatabase(TransferVM.ImportDatabase idb, Action<NotifyCollectionChangedEventArgs> le = null)
        {
            var vm = new SharedResultVM();
            vm.LogEvent(le);

            vm.Log.Add($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss} 导入数据\n");

            var db = idb.WriteConnectionInfo.NewDbHelper();

            var hsName = new HashSet<string>();

            vm.Log.Add($"读取数据源：{idb.ZipPath}\n");

            using var zipRead = ZipFile.OpenRead(idb.ZipPath);
            var zipList = zipRead.Entries.OrderBy(x => x.Name).ToList();

            vm.Log.Add($"读取写入库表信息");
            var dk = Init(idb.WriteConnectionInfo);
            var writeTables = dk.GetTable();

            for (int i = 0; i < zipList.Count; i++)
            {
                var dt = new DataTable();

                var item = zipList[i];
                dt.ReadXml(item.Open());

                //指定导入表名
                var writeTableMap = string.Empty;
                foreach (var key in idb.ReadWriteTableMap.Keys)
                {
                    var val = idb.ReadWriteTableMap[key];
                    var sntnArray = key.Split('.');
                    if (sntnArray.Length == 2)
                    {
                        if (dt.Namespace == sntnArray[0] && dt.TableName == sntnArray[1])
                        {
                            writeTableMap = val;
                            break;
                        }
                    }
                    else if (dt.TableName == key)
                    {
                        writeTableMap = val;
                        break;
                    }
                }
                //指定映射 且在 写入库
                if (!string.IsNullOrWhiteSpace(writeTableMap) && writeTables.Any(x => DbHelper.SqlEqualSNTN(writeTableMap, x.TableName, x.SchemaName)))
                {
                    var sntnArray = writeTableMap.Split('.');
                    if (sntnArray.Length == 2)
                    {
                        dt.Namespace = sntnArray[0];
                        dt.TableName = sntnArray[1];
                    }
                    else
                    {
                        dt.Namespace = "";
                        dt.TableName = sntnArray[0];
                    }
                }
                //有模式名 但不在 写入库 清除模式名
                else if (!string.IsNullOrWhiteSpace(dt.Namespace) && !writeTables.Any(x => x.SchemaName == dt.Namespace))
                {
                    dt.Namespace = "";
                }

                if (!writeTables.Any(x => x.TableName == dt.TableName))
                {
                    vm.Log.Add($"写入库未找到表（{dt.TableName}），已跳过");
                    continue;
                }

                //模式名.表名
                var sntn = DbHelper.SqlSNTN(dt.TableName, dt.Namespace);

                //清空表
                if (hsName.Add(sntn) && idb.WriteDeleteData)
                {
                    var clearTableSql = DbHelper.SqlClearTable(idb.WriteConnectionInfo.ConnectionType, sntn);

                    vm.Log.Add($"清理写入表：{clearTableSql}");
                    var num = db.SqlExecuteReader(clearTableSql).Item2;
                    vm.Log.Add($"返回受影响行数：{num}，耗时：{vm.PartTimeFormat()}");
                }

                vm.Log.Add($"导入表（{sntn}）分片：{item.Name}（大小：{ParsingTo.FormatByteSize(item.Length)}，行：{dt.Rows.Count}）");

                switch (idb.WriteConnectionInfo.ConnectionType)
                {
                    case SharedEnum.TypeDB.SQLite:
                        db.BulkBatchSQLite(dt);
                        break;
                    case SharedEnum.TypeDB.MySQL:
                    case SharedEnum.TypeDB.MariaDB:
                        db.BulkCopyMySQL(dt);
                        break;
                    case SharedEnum.TypeDB.Oracle:
                        db.BulkCopyOracle(dt);
                        break;
                    case SharedEnum.TypeDB.SQLServer:
                        db.BulkCopySQLServer(dt);
                        break;
                    case SharedEnum.TypeDB.PostgreSQL:
                        db.BulkKeepIdentityPostgreSQL(dt);
                        break;
                }

                vm.Log.Add($"导入表（{sntn}）分片成功，耗时：{vm.PartTimeFormat()}，导入进度：{i + 1}/{zipList.Count}\n");
            }

            vm.Log.Add($"导入完成，共耗时：{vm.UseTimeFormat}\n");
            GC.Collect();

            vm.Set(SharedEnum.RTag.success);
            return vm;
        }

    }
}

#endif