using System;
using System.Reflection;
using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

/// <summary>
/// 全局
/// </summary>
public class GlobalTo
{
    /// <summary>
    /// 全局配置
    /// </summary>
    public static IConfiguration Configuration;

    /// <summary>
    /// 托管环境信息
    /// </summary>
    public static IHostEnvironment HostEnvironment;

    /// <summary>
    /// 内部访问（项目根路径）
    /// </summary>
    public static string ContentRootPath
    {
        get
        {
            return HostEnvironment.ContentRootPath;
        }
    }

    /// <summary>
    /// web外部访问（wwwroot）
    /// </summary>
    public static string WebRootPath
    {
        get
        {
            return ContentRootPath + "/wwwroot";
        }
    }

    /// <summary>
    /// 起始路径（Windows为磁盘跟目录，linux为/）
    /// </summary>
    public static string StartPath = "/";

    /// <summary>
    /// 获取AppsettingsJson的值
    /// </summary>
    /// <param name="key">键路径，如：ConnectionStrings:SQLServerConn</param>
    /// <returns></returns>
    public static string GetValue(string key)
    {
        return Configuration.GetValue<string>(key);
    }

    /// <summary>
    /// 获取AppsettingsJson的值
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="key">键路径</param>
    /// <returns></returns>
    public static T GetValue<T>(string key)
    {
        return Configuration.GetValue<T>(key);
    }

    /// <summary>
    /// 值类型转换
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="value">值</param>
    /// <returns></returns>
    public static T ConvertValue<T>(string value)
    {
        return (T)ConvertValue(typeof(T), value);
    }

    /// <summary>
    /// 值类型转换
    /// </summary>
    /// <param name="type">类型</param>
    /// <param name="value">值</param>
    /// <returns></returns>
    public static object ConvertValue(Type type, string value)
    {
        if (type == typeof(object))
        {
            return value;
        }

        if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            return ConvertValue(Nullable.GetUnderlyingType(type), value);
        }

        var converter = TypeDescriptor.GetConverter(type);
        if (converter.CanConvertFrom(typeof(string)))
        {
            return converter.ConvertFromInvariantString(value);
        }

        return null;
    }
}