using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;

namespace Netnr.FileServer
{
    /// <summary>
    /// 
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="configuration">配置信息</param>
        /// <param name="env">环境信息</param>
        public Startup(IConfiguration configuration, IHostEnvironment env)
        {
            GlobalTo.Configuration = configuration;
            GlobalTo.HostEnvironment = env;
        }

        /// This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                //Action原样输出JSON
                options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                //日期格式化
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            });

            //配置swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "FileServer API",
                    Version = "v1"
                });

                "FileServer,Fast".Split(',').ToList().ForEach(x =>
                {
                    c.IncludeXmlComments(System.AppContext.BaseDirectory + "Netnr." + x + ".xml", true);
                });
            });

            //路由小写
            services.AddRouting(options => options.LowercaseUrls = true);

            //配置上传文件大小限制（详细信息：FormOptions）
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = GlobalTo.GetValue<int>("StaticResource:MaxSize") * 1024 * 1024;
            });
        }

        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IMemoryCache memoryCache)
        {
            //缓存
            Core.CacheTo.memoryCache = memoryCache;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            //配置swagger
            app.UseSwagger().UseSwaggerUI(c =>
            {
                c.DocumentTitle = "FileServer API";
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "FileServer API");
            });

            //静态资源允许跨域
            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = (x) =>
                {
                    x.Context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                }
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
