using System;
using System.Collections.Generic;

namespace Netnr.Func.ViewModel
{
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
        public DateTime? DsdCreateTime { get; set; }
        public DateTime? DsdUpdateTime { get; set; }
    }

    public class DocTreeVM
    {
        public string DsdId { get; set; }
        public string DsdPid { get; set; }
        public string DsCode { get; set; }
        public string DsdTitle { get; set; }
        public int? DsdOrder { get; set; }
        /// <summary>
        /// 是目录
        /// </summary>
        public bool IsCatalog { get; set; }
    }
}
