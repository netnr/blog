namespace Netnr.Blog.Web.Areas.Doc.Controllers
{
    [Area("Doc")]
    public class HomeController : Controller
    {
        /// <summary>
        /// Doc首页
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }
    }
}