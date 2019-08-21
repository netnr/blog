using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Netnr.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        //dotnet Netnr.Web.dll "http://*:50"

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .UseUrls(args)
            .UseStartup<Startup>();
    }
}
