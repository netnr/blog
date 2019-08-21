using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Netnr.Data;
using Netnr.Login;

namespace Netnr.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            GlobalTo.StartTime = System.DateTime.Now;
            GlobalTo.Configuration = configuration;
            GlobalTo.HostingEnvironment = env;

            #region 第三方登录
            QQConfig.APPID = GlobalTo.GetValue("OAuthLogin:QQ:APPID");
            QQConfig.APPKey = GlobalTo.GetValue("OAuthLogin:QQ:APPKey");
            QQConfig.Redirect_Uri = GlobalTo.GetValue("OAuthLogin:QQ:Redirect_Uri");

            WeiboConfig.AppKey = GlobalTo.GetValue("OAuthLogin:Weibo:AppKey");
            WeiboConfig.AppSecret = GlobalTo.GetValue("OAuthLogin:Weibo:AppSecret");
            WeiboConfig.Redirect_Uri = GlobalTo.GetValue("OAuthLogin:Weibo:Redirect_Uri");

            GitHubConfig.ClientID = GlobalTo.GetValue("OAuthLogin:GitHub:ClientID");
            GitHubConfig.ClientSecret = GlobalTo.GetValue("OAuthLogin:GitHub:ClientSecret");
            GitHubConfig.Redirect_Uri = GlobalTo.GetValue("OAuthLogin:GitHub:Redirect_Uri");
            GitHubConfig.ApplicationName = GlobalTo.GetValue("OAuthLogin:GitHub:ApplicationName");

            TaobaoConfig.AppKey = GlobalTo.GetValue("OAuthLogin:Taobao:AppKey");
            TaobaoConfig.AppSecret = GlobalTo.GetValue("OAuthLogin:Taobao:AppSecret");
            TaobaoConfig.Redirect_Uri = GlobalTo.GetValue("OAuthLogin:Taobao:Redirect_Uri");

            MicroSoftConfig.ClientID = GlobalTo.GetValue("OAuthLogin:MicroSoft:ClientID");
            MicroSoftConfig.ClientSecret = GlobalTo.GetValue("OAuthLogin:MicroSoft:ClientSecret");
            MicroSoftConfig.Redirect_Uri = GlobalTo.GetValue("OAuthLogin:MicroSoft:Redirect_Uri");
            #endregion

            try
            {
                //无创建，有忽略
                using (var db = new ContextBase())
                {
                    db.Database.EnsureCreated();
                }
            }
            catch (System.Exception)
            {
            }
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddMvc(options =>
            {
                //注册全局错误过滤器
                options.Filters.Add(new Filters.FilterConfigs.ErrorActionFilter());

                //注册全局过滤器
                options.Filters.Add(new Filters.FilterConfigs.GlobalFilter());

                //注册全局授权访问时登录标记是否有效
                options.Filters.Add(new Filters.FilterConfigs.LogonSignValid());
            }).AddJsonOptions(options =>
            {
                //Action原样输出JSON
                options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                //日期格式化
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            });

            //cookie共享
            //services.AddDataProtection()
            //    .SetApplicationName("cookieshare")
            //    .PersistKeysToFileSystem(new DirectoryInfo(GlobalTo.StartPath + ".aspnetcore-cookieshare"));

            //授权访问信息
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                options.Cookie.Name = "NetnrAuth";
                options.LoginPath = new PathString("/account/login");
                options.AccessDeniedPath = new PathString("/account/login");
                options.ExpireTimeSpan = System.DateTime.Now.AddDays(10) - System.DateTime.Now;

                string cd = GlobalTo.GetValue("AuthCookieDomain");
                if (!string.IsNullOrWhiteSpace(cd))
                {
                    options.Cookie.Domain = cd;
                }
            });

            //session
            services.AddSession();

            //定时任务
            FluentScheduler.JobManager.Initialize(new Func.TaskAid.TaskComponent.Reg());

            //跨域（ 用法：[EnableCors("Cors")] ）
            services.AddCors(options =>
            {
                options.AddPolicy("Cors", builder =>
                {
                    //允许任何来源的主机访问
                    builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
                    //指定处理cookie
                });
            });

            //配置上传文件大小限制（详细信息：FormOptions）
            services.Configure<FormOptions>(options =>
            {
                //100MB
                options.ValueLengthLimit = 1024 * 1024 * 100;
                options.MultipartBodyLengthLimit = options.ValueLengthLimit;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IMemoryCache memoryCache)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            //默认起始页index.html
            DefaultFilesOptions options = new DefaultFilesOptions();
            options.DefaultFileNames.Add("index.html");
            app.UseDefaultFiles(options);

            //静态资源允许跨域
            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = (x) =>
                {
                    x.Context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                }
            });
            app.UseCookiePolicy();

            //授权访问
            app.UseAuthentication();

            //session
            app.UseSession();

            //跨域
            app.UseCors("Cors");

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                   name: "default",
                  template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                   name: "U",
                  template: "{controller=U}/{id}", new { action = "index" });

                routes.MapRoute(
                    name: "areas",
                    template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute("Code", "{area:exists}/{controller=Code}/{id?}/{sid?}", new { action = "index" });
                routes.MapRoute("Raw", "{area:exists}/{controller=Raw}/{id?}", new { action = "index" });
                routes.MapRoute("User", "{area:exists}/{controller=User}/{id?}", new { action = "index" });
            });

            //缓存
            Core.CacheTo.memoryCache = memoryCache;
        }
    }
}
