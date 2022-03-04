#if Full || AdoFull || AdoOracle

using Oracle.ManagedDataAccess.Client;

namespace Netnr.SharedAdo
{
    /// <summary>
    /// Oracle操作类
    /// </summary>
    public partial class DbHelper
    {
        /// <summary>
        /// 表批量写入
        /// </summary>
        /// <param name="dt">数据表</param>
        /// <param name="bulkCopy">设置表复制对象</param>
        /// <returns></returns>
        public int BulkCopyOracle(DataTable dt, Action<OracleBulkCopy> bulkCopy = null)
        {
            return SafeConn(() =>
            {
                var connection = (OracleConnection)Connection;
                using var bulk = new OracleBulkCopy(connection)
                {
                    DestinationTableName = dt.TableName,
                    BatchSize = dt.Rows.Count,
                    BulkCopyTimeout = 3600
                };

                bulkCopy?.Invoke(bulk);

                foreach (DataColumn dc in dt.Columns)
                {
                    bulk.ColumnMappings.Add(dc.ColumnName, dc.ColumnName);
                }

                bulk.WriteToServer(dt);

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
        public int BulkBatchOracle(DataTable dt, string sqlEmpty = null, Action<OracleDataAdapter> dataAdapter = null, bool openTransaction = true)
        {
            return SafeConn(() =>
            {
                var connection = (OracleConnection)Connection;
                OracleTransaction transaction = openTransaction ? (OracleTransaction)(Transaction = connection.BeginTransaction()) : null;

                var cb = new OracleCommandBuilder();
                if (string.IsNullOrWhiteSpace(sqlEmpty))
                {
                    var sntn = SqlSNTN(dt.TableName, dt.Namespace, SharedEnum.TypeDB.Oracle);
                    sqlEmpty = SqlEmpty(sntn);
                }

                cb.DataAdapter = new OracleDataAdapter
                {
                    SelectCommand = new OracleCommand(sqlEmpty, connection)
                };
                cb.ConflictOption = ConflictOption.OverwriteChanges;

                var da = new OracleDataAdapter
                {
                    InsertCommand = cb.GetInsertCommand(true),
                    UpdateCommand = cb.GetUpdateCommand(true)
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