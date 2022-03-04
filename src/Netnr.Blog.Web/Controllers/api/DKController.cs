namespace Netnr.Blog.Web.Controllers.api
{
    /// <summary>
    /// Netnr.DataKit API
    /// </summary>
    [Route("api/v1/dk/[action]")]
    [ResponseCache(Duration = 2)]
    [Apps.FilterConfigs.AllowCors]
    public class DKController : DataKitController
    {

    }
}