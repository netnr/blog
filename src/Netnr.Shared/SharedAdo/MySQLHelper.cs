#if Full || AdoFull || AdoMySQL

using MySqlConnector;

namespace Netnr.SharedAdo
{
    /// <summary>
    /// MySQL操作类
    /// </summary>
    public partial class DbHelper
    {
        /// <summary>
        /// 表批量写入
        /// </summary>
        /// <param name="dt">数据表</param>
        /// <param name="bulkCopy">设置表复制对象</param>
        /// <param name="openTransaction">开启事务，默认不开启</param>
        /// <returns></returns>
        public int BulkCopyMySQL(DataTable dt, Action<MySqlBulkCopy> bulkCopy = null, bool openTransaction = true)
        {
            return SafeConn(() =>
            {
                var connection = (MySqlConnection)Connection;
                MySqlTransaction transaction = openTransaction ? (MySqlTransaction)(Transaction = connection.BeginTransaction()) : null;

                var bulk = new MySqlBulkCopy(connection, transaction)
                {
                    DestinationTableName = dt.TableName,
                    BulkCopyTimeout = 3600
                };

                bulkCopy?.Invoke(bulk);

                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    bulk.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(i, dt.Columns[i].ColumnName));
                }

                bulk.WriteToServer(dt);

                transaction?.Commit();

                return dt.Rows.Count;
            });
        }

        /// <summary>
        /// 表批量写入
        /// 根据行数据 RowState 状态新增、修改
        /// </summary>
        /// <param name="dt">数据表</param>
        /// <param name="sqlEmpty">查询空表脚本，默认*，可选列，会影响数据更新的列</param>
        /// <param name="dataAdapter">执行前修改（命令行脚本、超时等信息）</param>
        /// <param name="openTransaction">开启事务，默认开启</param>
        /// <returns></returns>
        public int BulkBatchMySQL(DataTable dt, string sqlEmpty = null, Action<MySqlDataAdapter> dataAdapter = null, bool openTransaction = true)
        {
            return SafeConn(() =>
            {
                var connection = (MySqlConnection)Connection;
                MySqlTransaction transaction = openTransaction ? (MySqlTransaction)(Transaction = connection.BeginTransaction()) : null;

                var cb = new MySqlCommandBuilder();
                if (string.IsNullOrWhiteSpace(sqlEmpty))
                {
                    var sntn = SqlSNTN(dt.TableName, dt.Namespace, SharedEnum.TypeDB.MySQL);
                    sqlEmpty = SqlEmpty(sntn);
                }

                cb.DataAdapter = new MySqlDataAdapter
                {
                    SelectCommand = new MySqlCommand(sqlEmpty, connection, transaction)
                };
                cb.ConflictOption = ConflictOption.OverwriteChanges;

                var da = new MySqlDataAdapter
                {
                    InsertCommand = (MySqlCommand)cb.GetInsertCommand(true),
                    UpdateCommand = (MySqlCommand)cb.GetUpdateCommand(true)
                };
                da.InsertCommand.CommandTimeout = 300;
                da.UpdateCommand.CommandTimeout = 300;

                //执行前修改
                dataAdapter?.Invoke(da);

                var num = da.Update(dt);

                transaction?.Commit();

                return num;
            });
        }
    }
}

#endif