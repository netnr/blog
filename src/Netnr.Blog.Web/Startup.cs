using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Netnr.Core;
using Netnr.Login;
using Netnr.SharedFast;
using Newtonsoft.Json.Converters;

namespace Netnr.Blog.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment env)
        {
            GlobalTo.Configuration = configuration;
            GlobalTo.HostEnvironment = env;

            SharedReady.ReadyTo.EncodingReg();
            SharedReady.ReadyTo.LegacyTimestamp();

            //结巴词典路径
            var jbPath = PathTo.Combine(GlobalTo.ContentRootPath, "db/jieba");
            if (!Directory.Exists(jbPath))
            {
                Directory.CreateDirectory(jbPath);
                try
                {
                    var dhost = "https://raw.githubusercontent.com/anderscui/jieba.NET/master/src/Segmenter/Resources/";
                    "prob_trans.json,prob_emit.json,idf.txt,pos_prob_start.json,pos_prob_trans.json,pos_prob_emit.json,char_state_tab.json".Split(',').ToList().ForEach(file =>
                    {
                        var fullPath = PathTo.Combine(jbPath, file);
                        HttpTo.DownloadSave(HttpTo.HWRequest(dhost + file), fullPath);
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            JiebaNet.Segmenter.ConfigManager.ConfigFileBaseDir = jbPath;

            #region 第三方登录
            new List<Type>
            {
                typeof(QQConfig),
                typeof(WeChatConfig),
                typeof(WeiboConfig),
                typeof(GitHubConfig),
                typeof(GiteeConfig),
                typeof(TaoBaoConfig),
                typeof(MicroSoftConfig),
                typeof(DingTalkConfig),
                typeof(GoogleConfig),
                typeof(AliPayConfig),
                typeof(StackOverflowConfig)
            }.ForEach(lc =>
            {
                var fields = lc.GetFields();
                foreach (var field in fields)
                {
                    if (!field.Name.StartsWith("API_"))
                    {
                        var cv = GlobalTo.GetValue($"OAuthLogin:{lc.Name.Replace("Config", "")}:{field.Name}");
                        field.SetValue(lc, cv);
                    }
                }
            });
            #endregion
        }

        //配置swagger
        public string ver = "v1";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                //cookie存储需用户同意，欧盟新标准，暂且关闭，否则用户没同意无法写入
                options.CheckConsentNeeded = context => false;

                if (!GlobalTo.GetValue<bool>("ReadOnly"))
                {
                    //允许其他站点携带授权Cookie访问，会出现伪造
                    //Chrome新版本必须启用HTTPS，安装命令：dotnet dev-certs https
                    options.MinimumSameSitePolicy = SameSiteMode.None;
                }
            });

            services.AddControllersWithViews(options =>
            {
                //注册全局错误过滤器
                options.Filters.Add(new Apps.FilterConfigs.ErrorActionFilter());

                //注册全局过滤器
                options.Filters.Add(new Apps.FilterConfigs.GlobalFilter());

                //注册全局授权访问时登录标记是否有效
                options.Filters.Add(new Apps.FilterConfigs.LoginSignValid());
            });

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                //Action原样输出JSON
                options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                //日期格式化
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss.fff";

                //swagger枚举显示名称
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
            });

            //配置swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(ver, new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = GlobalTo.HostEnvironment.ApplicationName,
                    Description = string.Join(" &nbsp; ", new List<string>
                    {
                        "<b>Source</b>：<a target='_blank' href='https://github.com/netnr'>https://github.com/netnr</a>",
                        "<b>Blog</b>：<a target='_blank' href='https://www.netnr.com'>https://www.netnr.com</a>"
                    })
                });
                //注释
                c.IncludeXmlComments(AppContext.BaseDirectory + GetType().Namespace + ".xml", true);
            });
            //swagger枚举显示名称
            services.AddSwaggerGenNewtonsoftSupport();

            //授权访问信息
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                if (!GlobalTo.GetValue<bool>("ReadOnly"))
                {
                    //允许其他站点携带授权Cookie访问，会出现伪造
                    //Chrome新版本必须启用HTTPS，安装命令：dotnet dev-certs https
                    options.Cookie.SameSite = SameSiteMode.None;
                }

                options.Cookie.Name = "netnr_auth";
                options.LoginPath = "/account/login";
            });

            //session
            services.AddSession();

            //数据库连接池
            services.AddDbContextPool<Data.ContextBase>(options =>
            {
                Data.ContextBaseFactory.CreateDbContextOptionsBuilder(options);
            }, 10);

            //定时任务
            if (!GlobalTo.GetValue<bool>("ReadOnly"))
            {
                FluentScheduler.JobManager.Initialize(new Apps.TaskService.Reg());
            }

            //配置上传文件大小限制（详细信息：FormOptions）
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = GlobalTo.GetValue<int>("StaticResource:MaxSize") * 1024 * 1024;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Data.ContextBase db)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. https://aka.ms/aspnetcore-hsts
                // dotnet dev-certs https --trust
                app.UseHsts();
            }

            var createScript = db.Database.GenerateCreateScript();
            if (GlobalTo.TDB == SharedEnum.TypeDB.PostgreSQL)
            {
                //https://www.npgsql.org/efcore/release-notes/6.0.html
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

                createScript = createScript.Replace(" datetime ", " timestamp ");
            }
            Console.WriteLine(createScript);

            //数据库不存在则创建，创建后返回true
            if (db.Database.EnsureCreated())
            {
                //导入数据库示例数据
                var vm = new Controllers.ServicesController().DatabaseImport("db/backup_demo.zip");
                Console.WriteLine(vm.ToJson(true));
            }

            //配置swagger
            app.UseSwagger().UseSwaggerUI(c =>
            {
                c.DocumentTitle = GlobalTo.HostEnvironment.ApplicationName;
                c.SwaggerEndpoint($"{ver}/swagger.json", c.DocumentTitle);
                c.InjectStylesheet("/Home/SwaggerCustomStyle");
            });

            //默认起始页index.html
            DefaultFilesOptions options = new();
            options.DefaultFileNames.Add("index.html");
            app.UseDefaultFiles(options);

            //静态资源允许跨域
            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = (x) =>
                {
                    x.Context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    x.Context.Response.Headers.Add("Cache-Control", "public, max-age=604800");
                },
                ServeUnknownFileTypes = true
            });

            //目录浏览&&公开访问
            if (GlobalTo.GetValue<bool>("ReadOnly"))
            {
                app.UseFileServer(new FileServerOptions()
                {
                    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(GlobalTo.WebRootPath),
                    //目录浏览链接
                    RequestPath = new PathString("/_"),
                    EnableDirectoryBrowsing = true,
                    EnableDefaultFiles = false
                });
            }

            app.UseRouting();

            //授权访问
            app.UseAuthentication();
            app.UseAuthorization();

            //session
            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute("U", "{controller=U}/{id}", new { action = "index" });
                endpoints.MapControllerRoute("areas", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute("Code", "{area:exists}/{controller=Code}/{id?}/{sid?}", new { action = "index" });
                endpoints.MapControllerRoute("Raw", "{area:exists}/{controller=Raw}/{id?}", new { action = "index" });
                endpoints.MapControllerRoute("User", "{area:exists}/{controller=User}/{id?}", new { action = "index" });
            });
        }
    }
}
