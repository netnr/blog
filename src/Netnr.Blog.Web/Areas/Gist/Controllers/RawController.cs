using Netnr.Blog.Data;

namespace Netnr.Blog.Web.Areas.Gist.Controllers
{
    [Area("Gist")]
    public class RawController : Controller
    {
        public ContextBase db;

        public RawController(ContextBase cb)
        {
            db = cb;
        }

        /// <summary>
        /// 原始数据
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            string result = string.Empty;

            string filename = string.Empty;
            string id = RouteData.Values["id"]?.ToString();
            if (!string.IsNullOrWhiteSpace(id))
            {
                var mo = db.Gist.FirstOrDefault(x => x.GistCode == id && x.GistStatus == 1 && x.GistOpen == 1);
                if (mo != null)
                {
                    result = mo.GistContent;
                    filename = mo.GistFilename;
                }
            }

            if (RouteData.Values["sid"]?.ToString() == "download")
            {
                return File(Encoding.Default.GetBytes(result), "text/plain", filename);
            }
            else
            {
                return Content(result);
            }
        }
    }
}