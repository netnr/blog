using System.ComponentModel;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Netnr.Data;

namespace Netnr.Web.Areas.Gist.Controllers
{
    [Area("Gist")]
    public class RawController : Controller
    {
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
                using var db = new ContextBase();
                var mo = db.Gist.Where(x => x.GistCode == id && x.GistStatus == 1 && x.GistOpen == 1).FirstOrDefault();
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