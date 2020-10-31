using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using demo.Models;
using demo.Tools;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.HttpOverrides;

namespace demo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            services.AddControllersWithViews();
            
            services.AddControllers().AddNewtonsoftJson();
            services.AddControllers().AddXmlSerializerFormatters();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            // services.AddSingleton<IActionSelector, CustomActionSelector>();

            services.Configure<RouteOptions>(options => {
                options.ConstraintMap.Add("primeint", typeof(MyRouteConstraint));
                options.ConstraintMap.Add(CultureSelectRouteConstraint.CultureKey, typeof(CultureSelectRouteConstraint));
            });

            services.AddMvc( options => {
                options.Conventions.Add(new ControllerNameAttributeConvention());

                options.CacheProfiles.Add("si.net", new Microsoft.AspNetCore.Mvc.CacheProfile {
                    Duration = 60
                });
            })
            .AddMvcLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)       // .en.resx
            .AddViewLocalization()  // 
            .AddDataAnnotationsLocalization()   // 
            .AddRazorOptions(options => {
                options.ViewLocationFormats.Add("/ExternalViews/{0}.cshtml");
                options.ViewLocationExpanders.Add(new ThemesViewLocationExpander("MojLayout"));
            });

            services.AddApiVersioning(options => {
                options.ReportApiVersions = true;
                // options.ApiVersionReader = new QueryStringApiVersionReader("api-ver");
                options.ApiVersionReader = new HeaderApiVersionReader("api-ver");
                // options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(2, 0);
            });

            // cache odpowiedzi
            services.AddResponseCaching(options => {
                options.MaximumBodySize *= 2;
                options.UseCaseSensitivePaths = true;
            });

            /*
            services.AddSwaggerGen( c => {
                c.SwaggerDoc("v1");
            }); 
            */

            string db_password = Environment.GetEnvironmentVariable("SA_PASSWORD");

            services.AddDistributedSqlServerCache(options => {
                options.ConnectionString = $@"Data Source=db;Initial Catalog=DistCache;User Id=sa; Password={db_password};";
                options.SchemaName = "dbo";
                options.TableName = "TestCache";

            });

            services.AddSession();

            // services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();
            // services.AddSingleton<ITempDataProvider, SessionStateTempDataProvider>();
            services.AddSingleton<ITempDataProvider, DBTempDataProvider>();

            // .resx
            services.AddLocalization(options => {
                options.ResourcesPath = "Resources";    // gdzie bedziemy trzymac slowniki
            });

            // ISO - narodowości
            // pl_PL -> polski, w dialekcie polskim narodowym
            //  

            var supportedCutltures = new List<CultureInfo> {
                new CultureInfo("pl"),
                new CultureInfo("en")
                
            };

            services.Configure<RequestLocalizationOptions>(options => {
                options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(supportedCutltures.First().Name, supportedCutltures.First().Name);
                options.SupportedCultures = supportedCutltures;
                options.SupportedUICultures = supportedCutltures;
                options.RequestCultureProviders = new[] {
                       new QueryStringRequestCultureProvider {QueryStringKey = "culture", Options = options} 
                };
            });
            /*
                

                new AcceptLanguageHeaderRequestCultureProvider {
                        Options = options
                    } 
            */

            
            /*
                new RouteDataRequestCultureProvider { RouteDataStringKey = "language" } // poszukać
            */
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();  // stacktrace
            }   
            else
            {
                app.UseExceptionHandler("/Home/Error"); // 4xx, 5xx
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
           // app.UseHttpsRedirection();
            app.UseStaticFiles();

            var options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(options.Value);

            app.UseRouting();

            app.UseResponseCaching(); // Hubert, lukasz mrugala, patryk poblocki, Dawid Wesołowski

            app.UseAuthorization();     // odcinal ze wzgleud na uprawnienia
            app.UseSession();

            // app.UseResponseCaching();   // Dominik Kubiaczyk, Mateusz buchajewicz

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                
            });

            /*
            app.UseSwagger();
            app.UseSwaggerUI(options => {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "SI.NET API v1");
            }); */
        }
    }
}
