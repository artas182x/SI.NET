using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using demo.Models;
using demo.Tools;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
            services.AddControllersWithViews();

            services.AddControllers().AddNewtonsoftJson();
            services.AddControllers().AddXmlSerializerFormatters();

            services.AddSwaggerGen();
            
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            // services.AddSingleton<IActionSelector, CustomActionSelector>();

            services.Configure<RouteOptions>(options => {
                options.ConstraintMap.Add("primeint", typeof(MyRouteConstraint));
            });

            services.AddMvc( options => {
                options.Conventions.Add(new ControllerNameAttributeConvention());
                
                options.CacheProfiles.Add("si.net", new Microsoft.AspNetCore.Mvc.CacheProfile {
                    Duration = 60
                });
            });

            services.AddApiVersioning(options => {
                options.ReportApiVersions = true;
                options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                // options.ApiVersionReader = new QueryStringApiVersionReader("api-ver");
                options.ApiVersionReader = new HeaderApiVersionReader("api-ver");
                // options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(2, 0);
            });

            services.AddVersionedApiExplorer(o =>
            {
                o.GroupNameFormat = "'v'VVV";
            });

            // cache odpowiedzi
            services.AddResponseCaching(options => {
                options.MaximumBodySize *= 2;
                options.UseCaseSensitivePaths = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseSwagger();

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
            app.UseHttpsRedirection();
            app.UseStaticFiles();

                app.UseSwaggerUI(options => {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "SI.NET API v1");
            });

            app.UseRouting();

            app.UseResponseCaching(); // Hubert, lukasz mrugala, patryk poblocki, Dawid Wesołowski

            app.UseAuthorization();     // odcinal ze wzgleud na uprawnienia

            // app.UseResponseCaching();   // Dominik Kubiaczyk, Mateusz buchajewicz

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

            });



        }
    }
}
