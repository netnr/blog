using Newtonsoft.Json;

/// <summary>
/// 输出信息
/// </summary>
public class QueryDataOutputVM
{
    /// <summary>
    /// 总条数
    /// </summary>
    [JsonProperty("total")]
    public int Total { get; set; } = 0;

    /// <summary>
    /// 数据
    /// </summary>
    [JsonProperty("data")]
    public object Data { get; set; } = new List<object>();

    /// <summary>
    /// 数据，data转换表，忽略序列化
    /// </summary>
    [JsonIgnore]
    public DataTable Table { get; set; } = null;

    /// <summary>
    /// 列标题
    /// </summary>
    [JsonProperty("columns")]
    public object Columns { get; set; } = new List<object>();

    /// <summary>
    /// 拓展信息 
    /// </summary>
    [JsonProperty("or1")]
    public string Or1 { get; set; } = "";

    /// <summary>
    /// 拓展信息
    /// </summary>
    [JsonProperty("or2")]
    public string Or2 { get; set; } = "";

    /// <summary>
    /// 拓展信息 
    /// </summary>
    [JsonProperty("or3")]
    public string Or3 { get; set; } = "";

    /// <summary>
    /// 查询SQL
    /// </summary>
    [JsonIgnore]
    public string QuerySql { get; set; }
}