namespace Netnr.Blog.Web.Controllers
{
    /// <summary>
    /// 对象存储
    /// </summary>
    [Apps.FilterConfigs.IsAdmin]
    public class StorageController : Controller
    {
        /// <summary>
        /// 腾讯对象存储
        /// </summary>
        /// <returns></returns>
        public IActionResult Tencent()
        {
            return View();
        }

        /// <summary>
        /// 网易对象存储
        /// </summary>
        /// <returns></returns>
        public IActionResult NetEasy()
        {
            return View();
        }

        /// <summary>
        /// 七牛对象存储
        /// </summary>
        /// <returns></returns>
        public IActionResult Qiniu()
        {
            ViewData["DateUnix"] = DateTime.Now.AddHours(1).ToTimestamp();
            return View();
        }

        /// <summary>
        /// 优刻得对象存储
        /// </summary>
        /// <returns></returns>
        public IActionResult UCloud()
        {
            return View();
        }
    }
}