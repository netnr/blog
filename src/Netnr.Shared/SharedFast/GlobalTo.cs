﻿#if Full || Fast

using System.Reflection;
using System.ComponentModel;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace Netnr.SharedFast
{
    /// <summary>
    /// 全局
    /// </summary>
    public class GlobalTo
    {
        /// <summary>
        /// 数据库类型
        /// </summary>
        public static SharedEnum.TypeDB TDB;

        /// <summary>
        /// 根据数据库类型获取连接字符串
        /// </summary>
        /// <returns></returns>
        public static string GetConn()
        {
            return Configuration.GetConnectionString(TDB.ToString());
        }

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
                return Path.Combine(ContentRootPath, "wwwroot");
            }
        }

        /// <summary>
        /// 获取AppsettingsJson的值
        /// </summary>
        /// <param name="key">键路径，如：ConnectionStrings:SQLServer</param>
        /// <returns></returns>
        public static string GetValue(string key)
        {
            return Configuration[key];
        }

        /// <summary>
        /// 获取AppsettingsJson的值
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="key">键路径</param>
        /// <returns></returns>
        public static T GetValue<T>(string key)
        {
            return ConvertValue<T>(GetValue(key));
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
}

#endif