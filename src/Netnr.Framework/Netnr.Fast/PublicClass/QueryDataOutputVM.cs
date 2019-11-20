using System.Data;
using Newtonsoft.Json;
using System.Collections.Generic;

/// <summary>
/// 输出信息
/// </summary>
public class QueryDataOutputVM
{
    /// <summary>
    /// 总条数
    /// </summary>
    public int total { get; set; } = 0;

    /// <summary>
    /// 数据
    /// </summary>
    public object data { get; set; } = new List<object>();

    /// <summary>
    /// 数据，data转换表，忽略序列化
    /// </summary>
    [JsonIgnore]
    public DataTable table { get; set; } = null;

    /// <summary>
    /// 列标题
    /// </summary>
    public object columns { get; set; } = new List<object>();

    /// <summary>
    /// 拓展信息 
    /// </summary>
    public string or1 { get; set; } = "";

    /// <summary>
    /// 拓展信息
    /// </summary>
    public string or2 { get; set; } = "";

    /// <summary>
    /// 拓展信息 
    /// </summary>
    public string or3 { get; set; } = "";
}