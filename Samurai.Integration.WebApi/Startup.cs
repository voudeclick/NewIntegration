using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Samurai.Integration.APIClient.Shopify;
using Samurai.Integration.Application.DependencyInjection;
using Samurai.Integration.Domain.Consts;
using Samurai.Integration.Email;
using Samurai.Integration.EntityFramework.Database;
using Samurai.Integration.EntityFramework.Repositories;
using Samurai.Integration.Identity.Data;
using Samurai.Integration.Identity.Models;
using Samurai.Integration.WebApi.Extensions;
using Samurai.Integration.WebApi.ServiceHangfire;
using Samurai.Integration.WebApi.Telemetry;

using Serilog;

using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.IO;
using System.Text;
using Samurai.Integration.WebApi.Filters;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Samurai.Integration.WebApi
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
            services.AddCors();
            services.AddControllers();

            services.AddDbContext<DatabaseContext>(options => options
                            .UseLazyLoadingProxies()
                            .UseSqlServer(Configuration.GetConnectionString("Database")));

            //services.AddDbContext<IdentityContext>(options =>
            //    options.UseSqlServer(Configuration.GetConnectionString("Database")));

            services.AddDistributedSqlServerCache(options =>
            {
                options.ConnectionString = Configuration.GetConnectionString("Database");
                options.SchemaName = "dbo";
                options.TableName = "IntegrationCache";
            });

            //services.AddIdentity<IdentityUser, IdentityRole>(options =>
            //{
            //    options.SignIn.RequireConfirmedAccount = true;
            //    options.User.AllowedUserNameCharacters = string.Empty;
            //}).AddRoleManager<RoleManager<IdentityRole>>()
            //  .AddEntityFrameworkStores<IdentityContext>();

            var key = Encoding.ASCII.GetBytes(Settings.Secret);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

#if !DEBUG
            services.AddApplicationInsightsTelemetry();
            if (Configuration["ShippingInsightsEnabled"] != "true")
                services.AddApplicationInsightsTelemetryProcessor<ShippingTelemetryFilter>();
#endif


            services.AddHangfire(x => x
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
           .UseSimpleAssemblyNameTypeSerializer()
           .UseRecommendedSerializerSettings()
           .UseSqlServerStorage(Configuration.GetConnectionString("Database"), new SqlServerStorageOptions
           {
               CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
               QueuePollInterval = TimeSpan.Zero,
               UseRecommendedIsolationLevel = true,
               UsePageLocksOnDequeue = true,
               DisableGlobalLocks = true
           }));

            services.AddHangfireServer();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1.0", new OpenApiInfo
                {
                    Title = Configuration?.GetSection("Swagger:Title")?.Value,
                    Version = Configuration?.GetSection("Swagger:Version")?.Value
                });

                string xmlPath = System.Reflection.Assembly.GetEntryAssembly().Location.Replace(".dll", ".xml");
                if (File.Exists(xmlPath))
                    c.IncludeXmlComments(xmlPath);
                // Include 'SecurityScheme' to use JWT Authentication
                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Name = "JWT Authentication",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",

                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };

                c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { jwtSecurityScheme, Array.Empty<string>() }
                });
            });

            services.AddHttpClient();
            services.Configure<EmailSettings>(Configuration.GetSection("EmailSettings"));
            services.Configure<ShopifySettings>(Configuration.GetSection("Shopify"));
            services.AddTransient<IntegrationMonitorHangfire>();
            services.AddSingleton<ClientIpCheckActionFilter>();
            DependencyInjectionStartup.ConfigureDI(services);

            Log.Logger = new LoggerConfiguration()
                                .ReadFrom.Configuration(Configuration)
                                .CreateLogger();

            services.AddMemoryCache();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            IWebHostEnvironment env,
            IHostApplicationLifetime lifetime,
            IDistributedCache cache)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            lifetime.ApplicationStarted.Register(() =>
            {
                var currentTimeUTC = DateTime.UtcNow.ToString();
                byte[] encodedCurrentTimeUTC = Encoding.UTF8.GetBytes(currentTimeUTC);
                var options = new DistributedCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));
                cache.Set("cachedTimeUTC", encodedCurrentTimeUTC, options);
            });

            app.UseRequestLogging();

            app.UseCors(
                options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
            );

            app.UseHttpsRedirection();

            app.Use(async (context, next) =>
            {
                context.Request.EnableBuffering();
                await next();
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(Configuration?.GetSection("Swagger:Endpoint")?.Value,
                                  Configuration?.GetSection("Swagger:Title")?.Value + " - " + Configuration?.GetSection("Swagger:Version")?.Value);
                c.DocumentTitle = Configuration?.GetSection("Swagger:Title")?.Value;
                c.DocExpansion(DocExpansion.None);
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHangfireDashboard();
            });

            app.UseHangfireDashboard();

            InitProcess(app.ApplicationServices);
        }

        private void InitProcess(IServiceProvider serviceProvider)
        {
            var clear = new Clear(serviceProvider);

            var cronExp = Cron.Weekly(DayOfWeek.Thursday);
            var scope = serviceProvider.CreateScope();

            var paramRepository = scope.ServiceProvider.GetRequiredService<ParamRepository>();
            var paramValue = paramRepository.GetByIntegrationMonitorHangfireKeyAsync()
                .Result?.GetValueBykey(IntegrationMonitorHangfireConsts.CronExpression);

            RecurringJob.AddOrUpdate(() => clear.Initialize(), cronExp);

            if (paramValue != null)
            {
                RecurringJob.AddOrUpdate<IntegrationMonitorHangfire>(x => x.Initialize(), paramValue.Value.ToString());
            }

        }
    }
}
