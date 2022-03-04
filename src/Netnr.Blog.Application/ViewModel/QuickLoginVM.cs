using Netnr.Core;
using Netnr.SharedFast;

namespace Netnr.Blog.Application.ViewModel
{
    /// <summary>
    /// 快捷登录
    /// </summary>
    public class QuickLoginVM
    {
        /// <summary>
        /// 路径名，标识
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string Icon
        {
            get
            {
                return PathTo.Combine(GlobalTo.GetValue("StaticResource:Server"), GlobalTo.GetValue("StaticResource:LoginPath"), Key + ".svg");
            }
        }

        /// <summary>
        /// 是否绑定
        /// </summary>
        public bool Bind { get; set; }
    }
}