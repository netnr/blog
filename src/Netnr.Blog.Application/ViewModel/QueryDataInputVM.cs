using Newtonsoft.Json;

/// <summary>
/// 输入参数
/// </summary>
public class QueryDataInputVM
{
    /// <summary>
    /// 处理类型，可选：query、export
    /// </summary>
    [JsonProperty("handleType")]
    public string HandleType { get; set; } = "query";

    /// <summary>
    /// 请求标识
    /// </summary>
    [JsonProperty("uri")]
    public string Uri { get; set; }

    /// <summary>
    /// 表名
    /// </summary>
    [JsonProperty("tableName")]
    public string TableName { get; set; } = "";

    /// <summary>
    /// 查询条件
    /// </summary>
    [JsonProperty("wheres")]
    public string Wheres { get; set; } = "";

    /// <summary>
    /// 是否启用分页 1分页
    /// </summary>
    [JsonProperty("pagination")]
    public int Pagination { get; set; } = 1;

    /// <summary>
    /// 页码 默认 1
    /// </summary>
    [JsonProperty("page")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// 页量 默认 30
    /// </summary>
    [JsonProperty("rows")]
    public int Rows { get; set; } = 30;

    /// <summary>
    /// 排序列名
    /// </summary>
    [JsonProperty("sort")]
    public string Sort { get; set; }

    /// <summary>
    /// 排序方式 默认 asc
    /// </summary>
    [JsonProperty("order")]
    public string Order { get; set; } = "asc";

    /// <summary>
    /// 排序拼接
    /// </summary>
    [JsonProperty("sortOrderJoin")]
    public string SortOrderJoin { get; set; }

    /// <summary>
    /// 是否查询列信息 1不查询
    /// </summary>
    [JsonProperty("columnsExists")]
    public int ColumnsExists { get; set; } = 0;

    /// <summary>
    /// 拓展参数 
    /// </summary>
    [JsonProperty("pe1")]
    public string Pe1 { get; set; }

    /// <summary>
    /// 拓展参数 
    /// </summary>
    [JsonProperty("pe2")]
    public string Pe2 { get; set; }

    /// <summary>
    /// 拓展参数 
    /// </summary>
    [JsonProperty("pe3")]
    public string Pe3 { get; set; }

}