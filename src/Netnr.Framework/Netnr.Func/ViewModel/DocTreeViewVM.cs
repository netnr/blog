using System;
using System.Collections.Generic;

namespace Netnr.Func.ViewModel
{
    /// <summary>
    /// 文档内容视图
    /// </summary>
    public class DocTreeViewVM
    {
        /// <summary>
        /// 文档集编码
        /// </summary>
        public string DsCode { get; set; }
        /// <summary>
        /// 文档集目录
        /// </summary>
        public List<DocTreeVM> DocTree { get; set; }
        /// <summary>
        /// 页编号
        /// </summary>
        public string DsdId { get; set; }
        /// <summary>
        /// 页标题
        /// </summary>
        public string DsdTitle { get; set; }
        /// <summary>
        /// 页内容
        /// </summary>
        public string DsdContentHtml { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? DsdCreateTime { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? DsdUpdateTime { get; set; }
    }

    /// <summary>
    /// 文档树形结构视图
    /// </summary>
    public class DocTreeVM
    {
        /// <summary>
        /// 文档页ID
        /// </summary>
        public string DsdId { get; set; }
        /// <summary>
        /// 父ID
        /// </summary>
        public string DsdPid { get; set; }
        /// <summary>
        /// 文档主码
        /// </summary>
        public string DsCode { get; set; }
        /// <summary>
        /// 文档页标题
        /// </summary>
        public string DsdTitle { get; set; }
        /// <summary>
        /// 文档页排序
        /// </summary>
        public int? DsdOrder { get; set; }
        /// <summary>
        /// 是目录
        /// </summary>
        public bool IsCatalog { get; set; }
    }
}
