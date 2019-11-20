/// <summary>
/// 输入参数
/// </summary>
public class QueryDataInputVM
{
    /// <summary>
    /// 处理类型，可选：query、export
    /// </summary>
    public string handleType { get; set; } = "query";

    /// <summary>
    /// 请求标识
    /// </summary>
    public string uri { get; set; }

    /// <summary>
    /// 表名
    /// </summary>
    public string tableName { get; set; } = "";

    /// <summary>
    /// 查询条件
    /// </summary>
    public string wheres { get; set; } = "";

    /// <summary>
    /// 是否启用分页 1分页
    /// </summary>
    public int pagination { get; set; } = 1;

    /// <summary>
    /// 页码 默认 1
    /// </summary>
    public int page { get; set; } = 1;

    /// <summary>
    /// 页量 默认 30
    /// </summary>
    public int rows { get; set; } = 30;

    /// <summary>
    /// 排序列名
    /// </summary>
    public string sort { get; set; }

    /// <summary>
    /// 排序方式 默认 asc
    /// </summary>
    public string order { get; set; } = "asc";

    /// <summary>
    /// 排序拼接
    /// </summary>
    public string sortOrderJoin { get; set; }

    /// <summary>
    /// 是否查询列信息 1不查询
    /// </summary>
    public int columnsExists { get; set; } = 0;

    /// <summary>
    /// 拓展参数 
    /// </summary>
    public string pe1 { get; set; }

    /// <summary>
    /// 拓展参数 
    /// </summary>
    public string pe2 { get; set; }

    /// <summary>
    /// 拓展参数 
    /// </summary>
    public string pe3 { get; set; }

}