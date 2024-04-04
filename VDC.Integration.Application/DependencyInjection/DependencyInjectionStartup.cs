using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using VDC.Integration.APIClient.ServiceBus;
using VDC.Integration.Application.Mappers;
using VDC.Integration.Application.Services;
using VDC.Integration.Domain.Infrastructure.Logs;
using VDC.Integration.Domain.Messages.ServiceBus;
using VDC.Integration.Domain.Repositories;
using VDC.Integration.Domain.Services;
using VDC.Integration.Domain.Services.Interfaces;
using VDC.Integration.EntityFramework.Repositories;
using VDC.Integration.EntityFramework.Repositories.Hangfire;
using VDC.Integration.EntityFramework.Repositories.Omie;

namespace VDC.Integration.Application.DependencyInjection
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
            services.AddTransient<OmieService, OmieService>();
            services.AddTransient<ParamService>();
            services.AddTransient<IntegrationErrorService>();
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
