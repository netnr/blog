#if Full || DataKit

using MySqlConnector;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Data.SqlClient;
using Npgsql;
using Netnr.SharedAdo;

namespace Netnr.SharedDataKit
{
    /// <summary>
    /// 数据交互基础方法
    /// </summary>
    public partial class DataKit
    {
        /// <summary>
        /// 数据库上下文
        /// </summary>
        public DbHelper db;

        /// <summary>
        /// 数据库类型
        /// </summary>
        public SharedEnum.TypeDB tdb;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="typeDB">类型</param>
        /// <param name="dbConnection">连接</param>
        public DataKit(SharedEnum.TypeDB typeDB, DbConnection dbConnection)
        {
            tdb = typeDB;
            db = new DbHelper(dbConnection);
        }

        /// <summary>
        /// 默认库名
        /// </summary>
        /// <returns></returns>
        public string DefaultDatabaseName()
        {
            var databaseName = db.Connection.Database;

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                if (db.Connection is OracleConnection connection)
                {
                    var sb = new OracleConnectionStringBuilder(connection.ConnectionString);
                    databaseName = sb.UserID;
                }
            }

            return databaseName;
        }

        /// <summary>
        /// 获取库名
        /// </summary>
        /// <returns></returns>
        public List<string> GetDatabaseName()
        {
            var result = new List<string>();

            var sql = DataKitScript.GetDatabaseName(tdb);
            var dt = db.SqlExecuteReader(sql).Item1.Tables[0];

            foreach (DataRow dr in dt.Rows)
            {
                if (tdb == SharedEnum.TypeDB.SQLite)
                {
                    result.Add(dr["name"].ToString());
                }
                else
                {
                    result.Add(dr[0].ToString());
                }
            }

            return result;
        }

        /// <summary>
        /// 获取库
        /// </summary>
        /// <param name="filterDatabaseName">过滤数据库名，逗号分隔</param>
        /// <returns></returns>
        public List<DatabaseVM> GetDatabase(string filterDatabaseName = null)
        {
            var result = new List<DatabaseVM>();

            var listDatabaseName = string.IsNullOrWhiteSpace(filterDatabaseName)
                ? null : filterDatabaseName.Replace("'", "").Split(',');

            var sql = DataKitScript.GetDatabase(tdb, listDatabaseName);
            var ds = db.SqlExecuteReader(sql);

            if (tdb == SharedEnum.TypeDB.SQLite)
            {
                var dt1 = ds.Item1.Tables[0];
                var charset = ds.Item1.Tables[1].Rows[0][0].ToString();
                foreach (DataRow dr in dt1.Rows)
                {
                    var name = dr["name"].ToString();
                    var file = dr["file"].ToString();
                    var fi = new FileInfo(file);

                    result.Add(new DatabaseVM
                    {
                        DatabaseName = name,
                        DatabaseCharset = charset,
                        DatabasePath = file,
                        DatabaseDataLength = fi.Length,
                        DatabaseCreateTime = fi.CreationTime
                    });
                }
            }
            else
            {
                result = ds.Item1.Tables[0].ToModel<DatabaseVM>();
            }

            return result;
        }

        /// <summary>
        /// 获取表
        /// </summary>
        /// <param name="schemaName">模式名</param>
        /// <param name="databaseName">数据库名</param>
        /// <returns></returns>
        public List<TableVM> GetTable(string schemaName = null, string databaseName = null)
        {
            var result = new List<TableVM>();

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                databaseName = DefaultDatabaseName();
            }

            var sql = DataKitScript.GetTable(tdb, databaseName, schemaName);
            var ds = db.SqlExecuteReader(sql);
            result = ds.Item1.Tables[0].ToModel<TableVM>();

            if (tdb == SharedEnum.TypeDB.SQLite)
            {
                //计算表行 https://stackoverflow.com/questions/4474873
                var listsql = new List<string> { "SELECT '' AS TableName, 0 AS TableRows" };
                result.ForEach(t =>
                {
                    var tableName = DbHelper.SqlQuote(SharedEnum.TypeDB.SQLite, t.TableName);
                    var sqlrows = $"SELECT '{t.TableName}' AS TableName, max(RowId) - min(RowId) + 1 AS TableRows FROM {tableName}";
                    listsql.Add(sqlrows);
                });
                var sqls = string.Join("\nUNION ALL\n", listsql);

                var dsrows = db.SqlExecuteReader(sqls).Item1.Tables[0].Rows.Cast<DataRow>();
                result.ForEach(item =>
                {
                    var trow = dsrows.FirstOrDefault(x => x[0].ToString().ToLower() == item.TableName.ToLower());
                    if (trow != null && trow[1].ToString() != "")
                    {
                        item.TableRows = Convert.ToInt64(trow[1]);
                    }
                });
            }

            return result;
        }

        /// <summary>
        /// 表DDL
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="schemaName">模式名</param>
        /// <param name="databaseName">数据库名</param>
        /// <returns></returns>
        public string GetTableDDL(string tableName, string schemaName = null, string databaseName = null)
        {
            string result = string.Empty;

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                databaseName = DefaultDatabaseName();
            }

            var sql = DataKitScript.GetTableDDL(tdb, databaseName, schemaName, tableName);
            Console.WriteLine(sql);
            switch (tdb)
            {
                case SharedEnum.TypeDB.SQLite:
                    {
                        var ds = db.SqlExecuteReader(sql);

                        var rows = ds.Item1.Tables[0].Rows;
                        var ddl = new List<string> { $"DROP TABLE IF EXISTS [{rows[0]["tbl_name"]}]" };
                        foreach (DataRow dr in rows)
                        {
                            var script = dr["sql"]?.ToString();
                            if (!string.IsNullOrWhiteSpace(script))
                            {
                                ddl.Add(script);
                            }
                        }

                        result = string.Join(";\r\n", ddl) + ";";
                    }
                    break;
                case SharedEnum.TypeDB.MySQL:
                case SharedEnum.TypeDB.MariaDB:
                    {
                        var ds = db.SqlExecuteReader(sql);

                        var rows = ds.Item1.Tables[0].Rows;
                        var ddl = new List<string> { $"DROP TABLE IF EXISTS `{rows[0][0]}`" };

                        var script = rows[0][1].ToString();
                        ddl.Add(script);

                        result = string.Join(";\r\n", ddl) + ";";
                    }
                    break;
                case SharedEnum.TypeDB.Oracle:
                    {
                        var ds = db.SqlExecuteReader(sql, func: cmd =>
                        {
                            var ocmd = (OracleCommand)cmd;

                            //begin ... end;
                            if (DbHelper.SqlParserBeginEnd(sql))
                            {
                                //open:name for
                                var cursors = DbHelper.SqlParserCursors(sql);
                                foreach (var cursor in cursors)
                                {
                                    ocmd.Parameters.Add(cursor, OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output);
                                }
                            }

                            return cmd;
                        });

                        var ddlTable = ds.Item1.Tables[0].Rows[0][0].ToString().Trim();
                        var ddlIndex = ds.Item1.Tables[1].Rows.Cast<DataRow>().Select(x => x[0].ToString().Trim() + ";");
                        var ddlCheck = ds.Item1.Tables[2].Rows.Cast<DataRow>().Select(x => x[0].ToString().Trim() + ";");
                        var ddlTableComment = ds.Item1.Tables[3].Rows[0][0].ToString().Trim();
                        var ddlColumnComment = ds.Item1.Tables[4];

                        var fullTableName = $"\"{databaseName}\".\"{tableName}\"";

                        var ddl = new List<string> { $"DROP TABLE {fullTableName};", $"{ddlTable};" };
                        if (ddlIndex.Any())
                        {
                            ddl.Add("");
                            ddl.AddRange(ddlIndex);
                        }
                        if (ddlCheck.Any())
                        {
                            ddl.Add("");
                            ddl.AddRange(ddlCheck);
                        }
                        ddl.Add("");
                        ddl.Add($"COMMENT ON TABLE {fullTableName} IS '{ddlTableComment.Replace("'", "''")}';");
                        ddl.Add("");
                        foreach (DataRow dr in ddlColumnComment.Rows)
                        {
                            ddl.Add($"COMMENT ON COLUMN {fullTableName}.\"{dr[0]}\" IS '{dr[1].ToString().Trim().Replace("'", "''")}';");
                        }

                        result = string.Join("\r\n", ddl);
                    }
                    break;
                case SharedEnum.TypeDB.SQLServer:
                    {
                        var ds = db.SqlExecuteReader(sql);

                        result = ds.Item1.Tables[0].Rows[0][1].ToString();
                    }
                    break;
                case SharedEnum.TypeDB.PostgreSQL:
                    {
                        //消息
                        var listInfo = new List<string>();
                        var dbConn = (NpgsqlConnection)db.Connection;
                        dbConn.Notice += (s, e) =>
                        {
                            listInfo.Add(e.Notice.MessageText);
                        };

                        db.SqlExecuteReader(sql);

                        result = string.Join("\r\n", listInfo);
                    }
                    break;
            }

            return result;
        }

        /// <summary>
        /// 获取列
        /// </summary>
        /// <param name="filterSchemaNameTableName">过滤模式表名，逗号分隔</param>
        /// <param name="databaseName">数据库名</param>
        /// <returns></returns>
        public List<ColumnVM> GetColumn(string filterSchemaNameTableName = null, string databaseName = null)
        {
            var result = new List<ColumnVM>();

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                databaseName = DefaultDatabaseName();
            }

            var listSchemaNameTableName = new List<Tuple<string, string>>();
            if (!string.IsNullOrWhiteSpace(filterSchemaNameTableName))
            {
                filterSchemaNameTableName.Replace("'", "").Split(',').ToList().ForEach(item =>
                {
                    var schemaName = string.Empty;
                    var tableName = string.Empty;
                    var listST = item.Split(".");
                    if (listST.Length == 2)
                    {
                        schemaName = listST[0];
                        tableName = listST[1];
                    }
                    else
                    {
                        tableName = listST[0];
                    }

                    listSchemaNameTableName.Add(new Tuple<string, string>(schemaName, tableName));
                });
            }

            var sql = DataKitScript.GetColumn(tdb, databaseName, listSchemaNameTableName);
            var ds = db.SqlExecuteReader(sql);

            if (tdb == SharedEnum.TypeDB.SQLite)
            {
                ds.Item1.Tables[0].Rows.RemoveAt(0);
                var ds2 = ds.Item1.Tables[1].Select();

                var aakey = "AUTOINCREMENT";
                foreach (DataRow dr in ds.Item1.Tables[0].Rows)
                {
                    var csql = ds2.FirstOrDefault(x => x["name"].ToString().ToLower() == dr["TableName"].ToString().ToLower())[1].ToString().ToUpper();
                    if (csql.Contains(aakey))
                    {
                        var isaa = csql.Split(',').Any(x => x.Contains(aakey) && x.Contains(dr["ColumnName"].ToString().ToUpper()));
                        if (isaa)
                        {
                            dr["AutoAdd"] = "YES";
                        }
                    }
                }
            }

            result = ds.Item1.Tables[0].ToModel<ColumnVM>();

            return result;
        }

        /// <summary>
        /// 设置表注释
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="tableComment">表注释</param>
        /// <param name="schemaName">模式名</param>
        /// <param name="databaseName">数据库名</param>
        /// <returns></returns>
        public bool SetTableComment(string tableName, string tableComment, string schemaName = null, string databaseName = null)
        {
            if (tdb != SharedEnum.TypeDB.SQLite)
            {
                if (string.IsNullOrWhiteSpace(databaseName))
                {
                    databaseName = DefaultDatabaseName();
                }

                var sql = DataKitScript.SetTableComment(tdb, databaseName, schemaName, tableName, tableComment);
                db.SqlExecuteNonQuery(sql);
            }

            return true;
        }

        /// <summary>
        /// 设置列注释
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="columnName">列名</param>
        /// <param name="columnComment">列注释</param>
        /// <param name="schemaName">模式名</param>
        /// <param name="databaseName">数据库名</param>
        /// <returns></returns>
        public bool SetColumnComment(string tableName, string columnName, string columnComment, string schemaName = null, string databaseName = null)
        {
            if (tdb != SharedEnum.TypeDB.SQLite)
            {
                if (string.IsNullOrWhiteSpace(databaseName))
                {
                    databaseName = DefaultDatabaseName();
                }

                var sql = DataKitScript.SetColumnComment(tdb, databaseName, schemaName, tableName, columnName, columnComment);
                db.SqlExecuteNonQuery(sql);
            }

            return true;
        }

        /// <summary>
        /// 执行脚本
        /// </summary>
        /// <param name="sql">脚本</param>
        /// <returns></returns>
        public Tuple<DataSet, DataSet, object> ExecuteSql(string sql)
        {
            var st = new SharedTimingVM();

            //消息
            var listInfo = new List<string>();

            var er = db.SqlExecuteReader(sql, func: cmd =>
            {
                switch (tdb)
                {
                    case SharedEnum.TypeDB.MySQL:
                    case SharedEnum.TypeDB.MariaDB:
                        {
                            var dbConn = (MySqlConnection)cmd.Connection;
                            dbConn.InfoMessage += (s, e) =>
                            {
                                listInfo.Add(e.Errors[0].Message);
                            };
                        }
                        break;
                    case SharedEnum.TypeDB.Oracle:
                        {
                            var dbCmd = (OracleCommand)cmd;
                            var dbConn = dbCmd.Connection;
                            dbConn.InfoMessage += (s, e) =>
                            {
                                listInfo.Add(e.Message);
                            };

                            //begin ... end;
                            if (DbHelper.SqlParserBeginEnd(sql))
                            {
                                //open:name for
                                var cursors = DbHelper.SqlParserCursors(sql);
                                foreach (var cursor in cursors)
                                {
                                    dbCmd.Parameters.Add(cursor, OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output);
                                }
                            }
                        }
                        break;
                    case SharedEnum.TypeDB.SQLServer:
                        {
                            var dbConn = (SqlConnection)cmd.Connection;
                            dbConn.InfoMessage += (s, e) =>
                            {
                                listInfo.Add(e.Message);
                            };
                        }
                        break;
                    case SharedEnum.TypeDB.PostgreSQL:
                        {
                            var dbConn = (NpgsqlConnection)cmd.Connection;
                            dbConn.Notice += (s, e) =>
                            {
                                listInfo.Add(e.Notice.MessageText);
                            };
                        }
                        break;
                }

                return cmd;
            });

            listInfo.Add($"耗时: {st.PartTimeFormat()}");
            st.sw.Stop();

            if (er.Item2 != -1)
            {
                listInfo.Insert(0, $"({er.Item2} 行受影响)");
            }

            var dtInfo = new DataTable();
            dtInfo.Columns.Add(new DataColumn("message"));
            listInfo.ForEach(info =>
            {
                var drInfo = dtInfo.NewRow();
                drInfo[0] = info;
                dtInfo.Rows.Add(drInfo.ItemArray);
            });

            return new Tuple<DataSet, DataSet, object>(er.Item1, er.Item3, new { info = dtInfo });
        }
    }
}

#endif