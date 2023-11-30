using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Samurai.Integration.Application.DependencyInjection;
using Samurai.Integration.EntityFramework.Database;
using Samurai.Integration.Identity.Data;
using Samurai.Integration.Identity.Models;

namespace Samurai.Integration.Front
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
            services.AddDbContext<DatabaseContext>(options => options                            
                            .UseSqlServer(Configuration.GetConnectionString("Database")));

            //services.AddDbContext<IdentityContext>(options =>
            //    options.UseSqlServer(Configuration.GetConnectionString("Database")));

            //services.AddDefaultIdentity<IdentityUser>(options =>
            //{
            //    options.SignIn.RequireConfirmedAccount = true;
            //    options.User.AllowedUserNameCharacters = string.Empty;
            //}).AddRoles<IdentityRole>()
            //  .AddRoleManager<RoleManager<IdentityRole>>()
            //  .AddEntityFrameworkStores<IdentityContext>();

            //services.Configure<CookiePolicyOptions>(options =>
            //{
            //    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            //    options.CheckConsentNeeded = context => true;
            //    options.MinimumSameSitePolicy = SameSiteMode.None;
            //});

            //services.ConfigureApplicationCookie(options =>
            //{
            //    options.Cookie.HttpOnly = true;                
            //    options.LoginPath = "/Identity/Account/Login";
            //    options.LogoutPath = "/Identity/Account/Logout";
            //    options.AccessDeniedPath = "/Identity/Account/AccessDenied";                
            //});

            //services.AddAuthentication()
            //    .AddGoogle(options =>
            //{
            //    var googleAuthNSection = Configuration.GetSection("GoogleAuthentication");

            //    options.ClientId = googleAuthNSection["ClientId"];
            //    options.ClientSecret = googleAuthNSection["ClientSecret"];
            //    options.ReturnUrlParameter = "/Admin/Dashboard";                
            //});

            services.AddRazorPages().AddRazorPagesOptions(options => {
                options.Conventions.AuthorizeFolder("/admin");
            });

            services.AddHttpClient();
            DependencyInjectionStartup.ConfigureDI(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
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
                endpoints.MapGet("/", context =>
                {
                    context.Response.Redirect("/Identity/Account/Login");
                    return Task.CompletedTask;
                });

                endpoints.MapRazorPages();
            });
        }
    }
}
