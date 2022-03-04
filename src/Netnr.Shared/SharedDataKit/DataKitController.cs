#if Full || DKController

using Netnr;
using Netnr.SharedDataKit;

/// <summary>
/// Netnr.DataKit API
/// </summary>
public class DataKitController : Controller
{
    /// <summary>
    /// 入口
    /// </summary>
    /// <param name="tdb"></param>
    /// <param name="conn"></param>
    /// <param name="databaseName"></param>
    /// <param name="dkCall"></param>
    /// <returns></returns>
    static SharedResultVM Entry(SharedEnum.TypeDB tdb, string conn, string databaseName, Func<DataKit, object> dkCall)
    {
        var vm = new SharedResultVM();

        try
        {
            var dk = DataKit.Init(tdb, conn, databaseName);
            if (dk != null)
            {
                vm.Data = dkCall(dk);
                vm.Set(SharedEnum.RTag.success);
            }
            else
            {
                vm.Set(SharedEnum.RTag.error);
            }
        }
        catch (Exception ex)
        {
            vm.Set(ex);
        }

        return vm;
    }

    /// <summary>
    /// 获取库名
    /// </summary>
    /// <param name="tdb">数据库类型</param>
    /// <param name="conn">连接字符串</param>
    /// <returns></returns>
    [HttpGet]
    public SharedResultVM GetDatabaseName(SharedEnum.TypeDB tdb, string conn)
    {
        return Entry(tdb, conn, null, dk => dk.GetDatabaseName());
    }

    /// <summary>
    /// 获取库
    /// </summary>
    /// <param name="tdb">数据库类型</param>
    /// <param name="conn">连接字符串</param>
    /// <param name="filterDatabaseName">数据库名</param>
    /// <returns></returns>
    [HttpGet]
    public SharedResultVM GetDatabase(SharedEnum.TypeDB tdb, string conn, string filterDatabaseName = null)
    {
        return Entry(tdb, conn, null, dk => dk.GetDatabase(filterDatabaseName));
    }

    /// <summary>
    /// 获取表
    /// </summary>
    /// <param name="tdb">数据库类型</param>
    /// <param name="conn">连接字符串</param>
    /// <param name="schemaName">模式名</param>
    /// <param name="databaseName">数据库名</param>
    /// <returns></returns>
    [HttpGet]
    public SharedResultVM GetTable(SharedEnum.TypeDB tdb, string conn, string schemaName = null, string databaseName = null)
    {
        return Entry(tdb, conn, databaseName, dk => dk.GetTable(schemaName, databaseName));
    }

    /// <summary>
    /// 表DDL
    /// </summary>
    /// <param name="tdb">数据库类型</param>
    /// <param name="conn">连接字符串</param>
    /// <param name="tableName">表名</param>
    /// <param name="schemaName">模式名</param>
    /// <param name="databaseName">数据库名</param>
    /// <returns></returns>
    [HttpGet]
    public SharedResultVM GetTableDDL(SharedEnum.TypeDB tdb, string conn, string tableName, string schemaName = null, string databaseName = null)
    {
        return Entry(tdb, conn, databaseName, dk => dk.GetTableDDL(tableName, schemaName, databaseName));
    }

    /// <summary>
    /// 获取列
    /// </summary>
    /// <param name="tdb">数据库类型</param>
    /// <param name="conn">连接字符串</param>
    /// <param name="filterSchemaNameTableName">过滤模式表名，逗号分隔</param>
    /// <param name="databaseName">数据库名</param>
    /// <returns></returns>
    [HttpPost]
    public SharedResultVM GetColumn([FromForm] SharedEnum.TypeDB tdb, [FromForm] string conn, [FromForm] string filterSchemaNameTableName = null, [FromForm] string databaseName = null)
    {
        return Entry(tdb, conn, databaseName, dk => dk.GetColumn(filterSchemaNameTableName, databaseName));
    }

    /// <summary>
    /// 设置表注释
    /// </summary>
    /// <param name="tdb">数据库类型</param>
    /// <param name="conn">连接字符串</param>
    /// <param name="tableName">表名</param>
    /// <param name="tableComment">表注释</param>
    /// <param name="schemaName">模式名</param>
    /// <param name="databaseName">数据库名</param>
    /// <returns></returns>
    [HttpPost]
    public SharedResultVM SetTableComment([FromForm] SharedEnum.TypeDB tdb, [FromForm] string conn, [FromForm] string tableName, [FromForm] string tableComment, [FromForm] string schemaName = null, [FromForm] string databaseName = null)
    {
        return Entry(tdb, conn, databaseName, dk => dk.SetTableComment(tableName, tableComment, schemaName, databaseName));
    }

    /// <summary>
    /// 设置列注释
    /// </summary>
    /// <param name="tdb">数据库类型</param>
    /// <param name="conn">连接字符串</param>
    /// <param name="tableName">表名</param>
    /// <param name="columnName">列名</param>
    /// <param name="columnComment">列注释</param>
    /// <param name="schemaName">模式名</param>
    /// <param name="databaseName">数据库名</param>
    /// <returns></returns>
    [HttpPost]
    public SharedResultVM SetColumnComment([FromForm] SharedEnum.TypeDB tdb, [FromForm] string conn, [FromForm] string tableName, [FromForm] string columnName, [FromForm] string columnComment, [FromForm] string schemaName = null, [FromForm] string databaseName = null)
    {
        return Entry(tdb, conn, databaseName, dk => dk.SetColumnComment(tableName, columnName, columnComment, schemaName, databaseName));
    }

    /// <summary>
    /// 执行脚本
    /// </summary>
    /// <param name="tdb">数据库类型</param>
    /// <param name="conn">连接字符串</param>
    /// <param name="sql">脚本</param>
    /// <param name="databaseName">数据库名</param>
    /// <returns></returns>
    [HttpPost]
    public SharedResultVM ExecuteSql([FromForm] SharedEnum.TypeDB tdb, [FromForm] string conn, [FromForm] string sql, [FromForm] string databaseName = null)
    {
        return Entry(tdb, conn, databaseName, dk => dk.ExecuteSql(sql));
    }
}

#endif