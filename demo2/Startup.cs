using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using demo2.Persistance;
using demo2.Persistance.Entities;
using demo2.Tools;
using GoogleReCaptcha.V3;
using GoogleReCaptcha.V3.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace demo2
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
            services.AddDbContext<PrimaryContext>(options =>
                                                  {
                                                      options.UseSqlServer(
                                                          Configuration.GetConnectionString("PrimaryContext"));
                                                  });
            services.AddIdentity<MyUserAccount, MyRole>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddEntityFrameworkStores<PrimaryContext>()
                    .AddRoleManager<RoleManager<MyRole>>()
                    .AddSignInManager<ApplicationSignInManager>()
                    .AddUserManager<UserManager<MyUserAccount>>()
                    .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
                                                {
                                                    options.Password.RequireDigit = true;
                                                    options.Password.RequireLowercase = true;
                                                    options.Password.RequiredUniqueChars = 1;

                                                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                                                    options.Lockout.MaxFailedAccessAttempts = 5;
                                                    options.Lockout.AllowedForNewUsers = true;

                                                    options.User.AllowedUserNameCharacters =
                                                        "abcdefghijklmnopqrstuvwxyz";
                                                    options.User.RequireUniqueEmail = true;
                                                });

            services.ConfigureApplicationCookie(options =>
                                                {
                                                    options.Cookie.HttpOnly = true;
                                                    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                                                    options.LoginPath = "/logowanie";
                                                    options.ReturnUrlParameter = "returnUrl";
                                                    options.AccessDeniedPath = "/nieudane-logowanie";
                                                    options.SlidingExpiration = true;
                                                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequirePermissionForDelete", policy => policy.RequireRole("Administrator"));
            });

            services.AddAuthentication(options =>
            {
                // options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                options.LoginPath = "/logowanie";
                options.AccessDeniedPath = "/nieudane-logowanie";
                options.ReturnUrlParameter = "returnUrl";
                options.SlidingExpiration = true;
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("1234567890123456")),
                    ValidateLifetime = true,
                    ClockSkew =  TimeSpan.FromMinutes(5)
                        
                };
            });

            services.AddHttpClient<ICaptchaValidator, GoogleReCaptchaValidator>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            UpgradeDatabase(app);
            CreateRolesAndUsers(serviceProvider).Wait();
            CreateProducts(app).Wait();
        }

        private async Task CreateProducts(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<PrimaryContext>();
                if (!context.Products.Any())
                {
                    List<Product> products = new List<Product>();
                    products.Add(new Product
                    {
                        Name = "MacBook Pro", Parameters = "Apple M1, 8GB RAM, 512 GB SSD",
                        Price = new decimal(7999.00)
                    });
                    products.Add(new Product
                    {
                        Name = "MacBook Pro", Parameters = "Apple M1, 8GB RAM, 256 GB SSD",
                        Price = new decimal(6999.00)
                    });
                    await context.Products.AddRangeAsync(products);
                    await context.SaveChangesAsync();
                }
            }
            
        }

        private async Task CreateRolesAndUsers(IServiceProvider serviceProvider)
        {
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<MyRole>>();
            var UserManager = serviceProvider.GetRequiredService<UserManager<MyUserAccount>>();

            string[] roleNames = {"Administrator", "Klient"};
            foreach (var roleName in roleNames)
            {
                var roleExist = await RoleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    MyRole myRole = new MyRole(roleName);
                    // myRole.Id = "bleble";
                    IdentityResult roleResult = await RoleManager.CreateAsync(myRole);
                }
            }

            var powerUser = new MyUserAccount()
            {
                UserName = "someuser",
                Email = "test@test.pl",
                EmailConfirmed = true,
                Nickname = "test"
            };
            string userPWD = "p1@P123";
            var _user = await UserManager.FindByEmailAsync("test@test.pl");
            if (_user == null)
            {
                var createPowerUser = await UserManager.CreateAsync(powerUser, userPWD);
                if (createPowerUser.Succeeded)
                {
                    await UserManager.AddToRoleAsync(powerUser, "Administrator");
                    var code = await UserManager.GenerateEmailConfirmationTokenAsync(powerUser);
                    var result = await UserManager.ConfirmEmailAsync(powerUser, code);
                }
            }

            var defaultUser = new MyUserAccount
            {
                UserName =  "default",
                Email = "xyz@xyz.pl",
                EmailConfirmed = true
            };
            var _defaultUser = await UserManager.FindByEmailAsync(defaultUser.Email);
            if (_defaultUser == null)
            {
                var createPowerUser = await UserManager.CreateAsync(defaultUser, userPWD);
                if (createPowerUser.Succeeded)
                {
                    await UserManager.AddToRoleAsync(defaultUser, "Klient");
                    var code = await UserManager.GenerateEmailConfirmationTokenAsync(defaultUser);
                    var result = await UserManager.ConfirmEmailAsync(defaultUser, code);
                }
            }

        }

        private void UpgradeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<PrimaryContext>();
                if (context != null && context.Database != null)
                {
                    context.Database.Migrate();
                }
            } 
        }
    }
}
