#if Full || Public

namespace Netnr
{
    /// <summary>
    /// 分页参数
    /// </summary>
    public class SharedPaginationVM
    {
        /// <summary>
        /// 页码
        /// </summary>
        public int PageNumber { get; set; }
        /// <summary>
        /// 页量
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 总数量
        /// </summary>
        public int Total { get; set; }
        /// <summary>
        /// 总页数
        /// </summary>
        public int PageTotal
        {
            get
            {
                int pt = 0;
                try
                {
                    pt = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(Total) / Convert.ToDecimal(PageSize)));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                return pt;
            }
        }
    }
}

#endif