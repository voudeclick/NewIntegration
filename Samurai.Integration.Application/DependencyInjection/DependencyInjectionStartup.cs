using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Samurai.Integration.APIClient.ServiceBus;
using Samurai.Integration.Application.Mappers;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Infrastructure.Email;
using Samurai.Integration.Domain.Infrastructure.Logs;
using Samurai.Integration.Domain.Messages.ServiceBus;
using Samurai.Integration.Domain.Repositories;
using Samurai.Integration.Domain.Services;
using Samurai.Integration.Domain.Services.Interfaces;
using Samurai.Integration.Email;
using Samurai.Integration.EntityFramework.Repositories;
using Samurai.Integration.EntityFramework.Repositories.Omie;
using Serilog;
using System;
using System.Net.Http;

namespace Samurai.Integration.Application.DependencyInjection
{
    public static class DependencyInjectionStartup
    {
        public static void ConfigureDI(IServiceCollection services)
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingMillenniumProfile());
            });
            IMapper mapper = mapperConfig.CreateMapper();

            services.AddSingleton(mapper);

            services.AddScoped<IMillenniumDomainService, MillenniumDomainService>();

            services.AddScoped<TenantRepository, TenantRepository>();
            services.AddScoped<TenantService, TenantService>();
            services.AddScoped<ITenantService, TenantService>();
            services.AddScoped<ServiceBusService, ServiceBusService>();
            services.AddScoped<WebhookService, WebhookService>();
            services.AddScoped<BlobService, BlobService>();
            services.AddScoped<MillenniumSessionToken, MillenniumSessionToken>();
            services.AddTransient<MillenniumService, MillenniumService>();
            services.AddTransient<ShopifyService, ShopifyService>();
            services.AddTransient<NexaasService, NexaasService>();
            services.AddTransient<OmieService, OmieService>();
            services.AddTransient<SellerCenterService, SellerCenterService>();
            services.AddTransient<Pier8Service, Pier8Service>();
            services.AddTransient<BlingService, BlingService>();
            services.AddTransient<PluggToService, PluggToService>();
            services.AddTransient<TrayService, TrayService>();
            services.AddTransient<EmailService>();
            services.AddTransient<ParamService>();
            services.AddTransient<IntegrationErrorService>();
            services.AddTransient<AliExpressService, AliExpressService>();
            services.AddTransient<MillenniumProductStockIntegrationRepository, MillenniumProductStockIntegrationRepository>();
            services.AddTransient<MillenniumNewStockProcessRepository, MillenniumNewStockProcessRepository>();
            services.AddTransient<MillenniumNewPricesProcessRepository, MillenniumNewPricesProcessRepository>();
            services.AddTransient<MillenniumProductPriceIntegrationRepository, MillenniumProductPriceIntegrationRepository>();
            services.AddTransient<MillenniumNewProductProcessRepository, MillenniumNewProductProcessRepository>();
            services.AddTransient<MillenniumProductIntegrationRepository, MillenniumProductIntegrationRepository>();
            services.AddTransient<MillenniumUpdateOrderProcessRepository, MillenniumUpdateOrderProcessRepository>();
            services.AddTransient<ShopifyListOrderProcessRepository, ShopifyListOrderProcessRepository>();
            services.AddTransient<ShopifyListOrderIntegrationRepository, ShopifyListOrderIntegrationRepository>();
            services.AddTransient<ShopifyUpdateOrderTagNumberProcessRepository, ShopifyUpdateOrderTagNumberProcessRepository>();
            services.AddTransient<ShopifyProductIntegrationRepository, ShopifyProductIntegrationRepository>();
            services.AddTransient<ShopifyProductPriceIntegrationRepository, ShopifyProductPriceIntegrationRepository>();
            services.AddTransient<ShopifyProductStockIntegrationRepository, ShopifyProductStockIntegrationRepository>();
            services.AddTransient<MillenniumProductImageIntegrationRepository, MillenniumProductImageIntegrationRepository>();
            services.AddTransient<MillenniumListProductManualProcessRepository, MillenniumListProductManualProcessRepository>();
            services.AddTransient<MillenniumImageIntegrationRepository, MillenniumImageIntegrationRepository>();
            services.AddTransient<ShopifyProductImageIntegrationRepository, ShopifyProductImageIntegrationRepository>();
            services.AddTransient<MillenniumOrderStatusUpdateRepository, MillenniumOrderStatusUpdateRepository>();
            services.AddTransient<ParamRepository>();
            services.AddTransient<IntegrationErrorRepository>();
            services.AddTransient<UserProfileRepository>();
            services.AddTransient(typeof(CleanTable<>), typeof(CleanTable<>));
            services.AddScoped<MillenniumSessionToken, MillenniumSessionToken>();
            services.AddTransient<IMethodPaymentRepository, MethodPaymentRepository>();
            services.AddTransient<IEmailClientSmtp, EmailClientSmtp>();
            services.AddTransient<LogsAbandonMessageRepository, LogsAbandonMessageRepository>();
            services.AddTransient<ILogServices, LogServices>();
            services.AddTransient<OmieOrderIntegrationRepository, OmieOrderIntegrationRepository>();

            services.AddScoped<MillenniumCreateOrderWithTotalDiscount>();
            services.AddScoped<MillenniumCreateOrderWithProductDiscount>();
            services.AddScoped<Func<string, ICreateOrder>>(serviceProvider => key =>
            {
                return key switch
                {
                    "totalDiscount" => serviceProvider.GetService<MillenniumCreateOrderWithTotalDiscount>(),
                    "productDiscount" => serviceProvider.GetService<MillenniumCreateOrderWithProductDiscount>(),
                    _ => null
                };
            });

            services.AddLogging(configure => configure.AddSerilog());
        }
        public static void ConfigureDIIntegrationSettings(IServiceCollection services)
        {
            services.AddSingleton(IntegrationSettings.Instancia);
        }
    }
}
