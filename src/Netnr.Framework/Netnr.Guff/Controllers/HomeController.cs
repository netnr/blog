using Microsoft.AspNetCore.Mvc;

namespace Netnr.Guff.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View("_PartialViewGuff");
        }

        /// <summary>
        /// 图片
        /// </summary>
        /// <returns></returns>
        public IActionResult Image()
        {
            return View("_PartialViewGuff", "img");
        }

        /// <summary>
        /// 声音
        /// </summary>
        /// <returns></returns>
        public IActionResult Audio()
        {
            return View("_PartialViewGuff", "audio");
        }

        /// <summary>
        /// 视频
        /// </summary>
        /// <returns></returns>
        public IActionResult Video()
        {
            return View("_PartialViewGuff", "video");
        }

        /// <summary>
        /// 排行
        /// </summary>
        /// <returns></returns>
        public IActionResult Top()
        {
            return View("_PartialViewGuff", "top");
        }

        /// <summary>
        /// 一条详情
        /// </summary>
        /// <returns></returns>
        public IActionResult Detail()
        {
            return View("_PartialViewGuff", "detail");
        }

        /// <summary>
        /// 发帖
        /// </summary>
        /// <returns></returns>
        public IActionResult Publish()
        {
            return View();
        }

        /// <summary>
        /// 我的
        /// </summary>
        /// <returns></returns>
        public IActionResult Me()
        {
            return View("_PartialViewGuffMe", "me");
        }

        /// <summary>
        /// 我点赞的
        /// </summary>
        /// <returns></returns>
        public IActionResult MeLaud()
        {
            return View("_PartialViewGuffMe", "melaud");
        }

        /// <summary>
        /// 我回复的
        /// </summary>
        /// <returns></returns>
        public IActionResult MeReply()
        {
            return View("_PartialViewGuffMe", "mereply");
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// 注销
        /// </summary>
        /// <returns></returns>
        public IActionResult Logout()
        {
            return View();
        }

        /// <summary>
        /// FAQ
        /// </summary>
        /// <returns></returns>
        public IActionResult FAQ()
        {
            return View();
        }
    }
}
