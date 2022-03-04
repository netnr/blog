using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Netnr.Core
{
    /// <summary>
    /// Tree常用方法
    /// </summary>
    public class TreeTo
    {
        /// <summary>
        /// 数据集合转JSON
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="pidField">父ID键</param>
        /// <param name="idField">ID键</param>
        /// <param name="startPid">开始的PID</param>
        /// <param name="childrenNodeName">子节点名称，默认children</param>
        /// <returns></returns>
        public static string ListToTree<T>(List<T> list, string pidField, string idField, List<string> startPid, string childrenNodeName = "children")
        {
            StringBuilder sbTree = new();

            var rdt = list.Where(x => startPid.Contains(x.GetType().GetProperty(pidField).GetValue(x, null).ToString())).ToList();

            for (int i = 0; i < rdt.Count; i++)
            {
                //数组“[”开始
                if (i == 0)
                {
                    sbTree.Append("[");
                }
                else
                {
                    sbTree.Append(",");
                }

                sbTree.Append("{");

                //数据行
                var dr = rdt[i];
                string mojson = dr.ToJson();
                sbTree.Append(mojson.TrimStart('{').TrimEnd('}'));

                var pis = dr.GetType().GetProperties();

                var pi = pis.FirstOrDefault(x => x.Name == idField);
                startPid.Clear();
                var id = pi.GetValue(dr, null).ToString();
                startPid.Add(id);

                var nrdt = list.Where(x => x.GetType().GetProperty(pidField).GetValue(x, null).ToString() == id.ToString()).ToList();

                if (nrdt.Count > 0)
                {
                    string rs = ListToTree(list, pidField, idField, startPid, childrenNodeName);

                    //子数组源于递归
                    sbTree.Append(",\"" + childrenNodeName + "\":" + rs + "}");
                }
                else
                {
                    sbTree.Append("}");
                }

                //数组结束“]”
                if (i == rdt.Count - 1)
                {
                    sbTree.Append("]");
                }
            }

            return sbTree.ToString();
        }

        /// <summary>
        /// 根据节点找到所有子节点（不包含自身节点）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="pidField">父ID键</param>
        /// <param name="idField">ID键</param>
        /// <param name="startPid">开始的PID</param>
        /// <returns></returns>
        public static List<T> FindToTree<T>(List<T> list, string pidField, string idField, List<string> startPid)
        {
            var outlist = new List<T>();

            var rdt = list.Where(x => startPid.Contains(x.GetType().GetProperty(pidField).GetValue(x, null).ToString())).ToList();

            for (int i = 0; i < rdt.Count; i++)
            {
                //数据行
                var dr = rdt[i];
                outlist.Add(dr);

                var pis = dr.GetType().GetProperties();

                var pi = pis.FirstOrDefault(x => x.Name == idField);
                startPid.Clear();
                var id = pi.GetValue(dr, null).ToString();
                startPid.Add(id);

                var nrdt = list.Where(x => x.GetType().GetProperty(pidField).GetValue(x, null).ToString() == id.ToString()).ToList();

                if (nrdt.Count > 0)
                {
                    var rs = FindToTree(list, pidField, idField, startPid);
                    outlist.AddRange(rs);
                }
            }

            return outlist;
        }
    }
}