using System;
using System.Data;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Netnr
{
    /// <summary>
    /// 常用方法拓展
    /// </summary>
    public static class Extend
    {
        /// <summary>
        /// object 转 JSON 字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="isSpace">缩进输出</param>
        /// <param name="DateTimeFormat">时间格式化</param>
        /// <returns></returns>
        public static string ToJson(this object obj, bool isSpace = false, string DateTimeFormat = "yyyy-MM-dd HH:mm:ss")
        {
            Newtonsoft.Json.Converters.IsoDateTimeConverter dtFmt = new()
            {
                DateTimeFormat = DateTimeFormat
            };
            return JsonConvert.SerializeObject(obj, isSpace ? Formatting.Indented : Formatting.None, dtFmt);
        }

        /// <summary>
        /// 解析 JSON字符串 为JObject对象
        /// </summary>
        /// <param name="json">JSON字符串</param>
        /// <returns>JObject对象</returns>
        public static JObject ToJObject(this string json)
        {
            return JObject.Parse(json);
        }

        /// <summary>
        /// 解析 JSON字符串 为JArray对象
        /// </summary>
        /// <param name="json">JSON字符串</param>
        /// <returns>JArray对象</returns>
        public static JArray ToJArray(this string json)
        {
            return JArray.Parse(json);
        }

        /// <summary>
        /// JSON字符串 转 类型
        /// </summary>
        /// <param name="json">JSON字符串</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static object ToType(this string json, Type type)
        {
            var mo = JsonConvert.DeserializeObject(json, type);
            return mo;
        }

        /// <summary>
        /// JSON字符串 转 实体
        /// </summary>
        /// <typeparam name="T">实体泛型</typeparam>
        /// <param name="json">JSON字符串</param>
        public static T ToEntity<T>(this string json)
        {
            var mo = JsonConvert.DeserializeObject<T>(json);
            return mo;
        }

        /// <summary>
        /// JSON字符串 转 实体
        /// </summary>
        /// <typeparam name="T">实体泛型</typeparam>
        /// <param name="json">JSON字符串</param>
        public static List<T> ToEntitys<T>(this string json)
        {
            var list = JsonConvert.DeserializeObject<List<T>>(json);
            return list;
        }

        /// <summary>
        /// 把jArray里面的json对象转为字符串
        /// </summary>
        /// <param name="jt">JToken对象</param>
        /// <returns></returns>
        public static string ToStringOrEmpty(this JToken jt)
        {
            try
            {
                return jt == null ? "" : jt.ToString();
            }
            catch (Exception)
            {
                return "";
            }
        }

        /// <summary>
        /// 字符串 JSON转义
        /// </summary>
        /// <param name="s">字符串</param>
        /// <returns></returns>
        public static string OfJson(this string s)
        {
            StringBuilder sb = new();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s.ToCharArray()[i];
                switch (c)
                {
                    case '\"':
                        sb.Append("\\\""); break;
                    case '\\':
                        sb.Append("\\\\"); break;
                    case '/':
                        sb.Append("\\/"); break;
                    case '\b':
                        sb.Append("\\b"); break;
                    case '\f':
                        sb.Append("\\f"); break;
                    case '\n':
                        sb.Append("\\n"); break;
                    case '\r':
                        sb.Append("\\r"); break;
                    case '\t':
                        sb.Append("\\t"); break;
                    default:
                        sb.Append(c); break;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// SQL单引号转义
        /// </summary>
        /// <param name="s">字符串</param>
        /// <returns></returns>
        public static string OfSql(this string s)
        {
            return s.Replace("'", "''");
        }

        /// <summary>
        /// 实体转表
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="list">对象</param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this IList<T> list)
        {
            Type elementType = typeof(T);
            var t = new DataTable();
            elementType.GetProperties().ToList().ForEach(propInfo => t.Columns.Add(propInfo.Name, Nullable.GetUnderlyingType(propInfo.PropertyType) ?? propInfo.PropertyType));
            foreach (T item in list)
            {
                var row = t.NewRow();
                elementType.GetProperties().ToList().ForEach(propInfo => row[propInfo.Name] = propInfo.GetValue(item, null) ?? DBNull.Value);
                t.Rows.Add(row);
            }
            return t;
        }

        /// <summary>
        /// 表转为实体
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="table">表</param>
        /// <returns></returns>
        public static List<T> ToModel<T>(this DataTable table) where T : class, new()
        {
            var list = new List<T>();
            foreach (DataRow dr in table.Rows)
            {
                var model = new T();
                var pis = model.GetType().GetProperties();
                foreach (DataColumn dc in dr.Table.Columns)
                {
                    object drValue = dr[dc.ColumnName];

                    var pi = pis.FirstOrDefault(x => x.Name.ToLower() == dc.ColumnName.ToLower());

                    Type type = pi.PropertyType;
                    if (pi.PropertyType.FullName.Contains("System.Nullable"))
                    {
                        type = Type.GetType("System." + pi.PropertyType.FullName.Split(',')[0].Split('.')[2]);
                    }

                    if (pi != null && pi.CanWrite && (drValue != null && drValue is not DBNull))
                    {
                        try
                        {
                            drValue = Convert.ChangeType(drValue, type);
                            pi.SetValue(model, drValue, null);
                        }
                        catch (Exception) { }
                    }
                }
                list.Add(model);
            }
            return list;
        }

        /// <summary>
        /// URL 编码
        /// </summary>
        /// <param name="value">内容</param>
        /// <returns></returns>
        public static string ToUrlEncode(this string value)
        {
            return System.Net.WebUtility.UrlEncode(value);
        }

        /// <summary>
        /// URL 解码
        /// </summary>
        /// <param name="value">内容</param>
        /// <returns></returns>
        public static string ToUrlDecode(this string value)
        {
            return System.Net.WebUtility.UrlDecode(value);
        }

        /// <summary>
        /// HTML 编码
        /// </summary>
        /// <param name="value">内容</param>
        /// <returns></returns>
        public static string ToHtmlEncode(this string value)
        {
            return System.Net.WebUtility.HtmlEncode(value);
        }

        /// <summary>
        /// HTML 解码
        /// </summary>
        /// <param name="value">内容</param>
        /// <returns></returns>
        public static string ToHtmlDecode(this string value)
        {
            return System.Net.WebUtility.HtmlDecode(value);
        }

        /// <summary>
        /// 转 Byte
        /// </summary>
        /// <param name="value">内容</param>
        /// <param name="encoding">默认 UTF8</param>
        /// <returns></returns>
        public static byte[] ToByte(this string value, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            return encoding.GetBytes(value);
        }

        /// <summary>
        /// Byte 转
        /// </summary>
        /// <param name="value">内容</param>
        /// <param name="encoding">默认 UTF8</param>
        /// <returns></returns>
        public static string ToText(this byte[] value, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            return encoding.GetString(value);
        }

        /// <summary>
        /// Base64 编码
        /// </summary>
        /// <param name="value">内容</param>
        /// <param name="encoding">默认 UTF8</param>
        /// <returns></returns>
        public static string ToBase64Encode(this string value, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            return Convert.ToBase64String(encoding.GetBytes(value));
        }

        /// <summary>
        /// Base64 解码
        /// </summary>
        /// <param name="value"></param>
        /// <param name="encoding">默认 UTF8</param>
        /// <returns></returns>
        public static string ToBase64Decode(this string value, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            return encoding.GetString(Convert.FromBase64String(value));
        }

        /// <summary>
        /// 对象值读取源
        /// </summary>
        /// <param name="target">需要赋值的对象</param>
        /// <param name="source">源对象</param>
        public static T ToRead<T>(this T target, object source) where T : class
        {
            var targetPis = target.GetType().GetProperties();
            var sourcePis = source.GetType().GetProperties();

            foreach (var sourcePi in sourcePis)
            {
                foreach (var targetPi in targetPis)
                {
                    if (targetPi.Name == sourcePi.Name)
                    {
                        var sourcePiVal = sourcePi.GetValue(source, null);
                        if (targetPi.PropertyType.IsAssignableFrom(sourcePi.PropertyType))
                        {
                            targetPi.SetValue(target, sourcePiVal, null);
                        }
                        else
                        {
                            targetPi.ToRead(sourcePiVal);
                        }
                        break;
                    }
                }
            }

            return target;
        }

        /// <summary>
        /// 将Datetime转换成时间戳，10位：秒 或 13位：毫秒
        /// </summary>
        /// <param name="datetime"></param>
        /// <param name="isms">毫秒，默认false为秒，设为true，返回13位，毫秒</param>
        /// <returns></returns>
        public static long ToTimestamp(this DateTime datetime, bool isms = false)
        {
            var t = datetime.ToUniversalTime().Ticks - 621355968000000000;
            var tc = t / (isms ? 10000 : 10000000);
            return tc;
        }

        /// <summary>
        /// 将Datetime转换成从UTC开始计算的总天数
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static int ToUtcTotalDays(this DateTime datetime)
        {
            var d = datetime.ToTimestamp() * 1.0 / 3600 / 24;
            return (int)Math.Ceiling(d);
        }

        /// <summary>
        /// 拓展批量添加
        /// </summary>
        /// <param name="oc"></param>
        /// <param name="list"></param>
        public static void AddRange(this ObservableCollection<object> oc, IEnumerable<object> list)
        {
            foreach (var item in list)
            {
                oc.Add(item);
            }
        }
    }
}