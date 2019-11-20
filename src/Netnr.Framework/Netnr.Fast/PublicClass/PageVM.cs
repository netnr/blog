using System.Collections.Generic;

/// <summary>
/// 分页视图
/// </summary>
public class PageVM
{
    /// <summary>
    /// 数据行
    /// </summary>
    public object Rows { get; set; }

    /// <summary>
    /// 临时数据
    /// </summary>
    public object Temp { get; set; }

    /// <summary>
    /// 其它数据
    /// </summary>
    public object Other { get; set; }

    /// <summary>
    /// 分页信息
    /// </summary>
    public PaginationVM Pag { get; set; }

    /// <summary>
    /// 路由 /home/index
    /// </summary>
    public string Route;

    /// <summary>
    /// url 传参 k=1
    /// </summary>
    public Dictionary<string, string> QueryString { get; set; }

    /// <summary>
    /// 分页 参数名 默认 page
    /// </summary>
    public string PageKeyName { get; set; } = "page";

    /// <summary>
    /// 生成页的地址
    /// </summary>
    /// <param name="pageIndex">页码</param>
    /// <returns></returns>
    public string Page(int pageIndex)
    {
        string up = string.Empty;
        if (QueryString != null)
        {
            foreach (string key in QueryString.Keys)
            {
                string val = QueryString[key];
                if (!string.IsNullOrWhiteSpace(val))
                {
                    up += "&" + key + "=" + System.Web.HttpUtility.UrlEncode(val);
                }
            }
        }
        if (up.Length > 2)
        {
            up = Route + "?" + up.TrimStart('&');
        }
        else
        {
            up = Route;
        }
        if (pageIndex != 1)
        {
            up = up + (up.Contains("?") ? "&" : "?") + PageKeyName + "=" + pageIndex;
        }
        return up;
    }
}