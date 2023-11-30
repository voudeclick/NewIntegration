using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Samurai.Integration.Domain.Entities;
using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.Domain.Entities.Database.Integrations.Millenium;
using Samurai.Integration.Domain.Entities.Database.Integrations.Omie;
using Samurai.Integration.Domain.Entities.Database.Integrations.Shopify;
using Samurai.Integration.Domain.Entities.Database.Logs;
using Samurai.Integration.EntityFramework.Configuration;

namespace Samurai.Integration.EntityFramework.Database
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext()
        {

        }

        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<ShopifyProductStockIntegration> ShopifyProductStockIntegrations { get; set; }
        public DbSet<MillenniumProductStockIntegration> MillenniumProductStockIntegrations { get; set; }
        public DbSet<MillenniumNewStockProcess> MillenniumNewStockProcesses { get; set; }
        public DbSet<MillenniumNewPricesProcess> MillenniumNewPricesProcesses { get; set; }
        public DbSet<MillenniumProductPriceIntegration> MillenniumProductPriceIntegrations { get; set; }        
        public DbSet<MillenniumNewProductProcess> MillenniumNewProductProcesses { get; set; }
        public DbSet<MillenniumProductIntegration> MillenniumProductIntegrations { get; set; }
        public DbSet<ShopifyProductIntegration> ShopifyProductIntegrations { get; set; }
        public DbSet<ShopifyProductPriceIntegration> ShopifyProductPriceIntegrations { get; set; }
        public DbSet<ShopifyListOrderProcess> ShopifyListOrderProcesses { get; set; }
        public DbSet<ShopifyListOrderIntegration> ShopifyListOrderIntegrations { get; set; }
        public DbSet<ShopifyUpdateOrderTagNumberProcess> ShopifyUpdateOrderTagNumberProcesses { get; set; }
        public DbSet<MillenniumUpdateOrderProcess> MillenniumUpdateOrderProcesses { get; set; }
        public DbSet<MillenniumProductImageIntegration> MillenniumProductImageIntegrations { get; set; }
        public DbSet<MillenniumListProductManualProcess> MillenniumListProductManualProcesses { get; set; }
        public DbSet<MillenniumImageIntegration> MillenniumImageIntegrations { get; set; }
        public DbSet<ShopifyProductImageIntegration> ShopifyProductImageIntegrations { get; set; }
        public DbSet<OmieUpdateOrderProcess> OmieUpdateOrderProcesses { get; set; }
        public DbSet<MethodPayment> MethodPayment { get; set; }
        public DbSet<Param> Params { get; set; }
        public DbSet<LogsAbandonMessage> LogsAbandonMessages { get; set; }
        public DbSet<IntegrationError> IntegrationErrors { get; set; }
        public DbSet<UserProfile> UsersProfile { get; set; }
        public DbSet<MillenniumOrderStatusUpdate> MillenniumOrderStatusUpdate { get; set; }
        public DbSet<MercadoPago> MercadoPago { get; set; } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new TenantConfiguration());
            modelBuilder.ApplyConfiguration(new ShopifyDataConfiguration());
            modelBuilder.ApplyConfiguration(new MillenniumDataConfiguration());
            modelBuilder.ApplyConfiguration(new MillenniumTransIdConfiguration());
            modelBuilder.ApplyConfiguration(new NexaasDataConfiguration());
            modelBuilder.ApplyConfiguration(new OmieDataConfiguration());
            modelBuilder.ApplyConfiguration(new SellerCenterTransIdConfiguration());
            modelBuilder.ApplyConfiguration(new LocationMapConfiguration());
            modelBuilder.ApplyConfiguration(new Pier8DataConfiguration());
            modelBuilder.ApplyConfiguration(new BlingDataConfiguration());
            modelBuilder.ApplyConfiguration(new ShopifyProductStockIntegrationConfiguration());
            modelBuilder.ApplyConfiguration(new MillenniumProductStockIntegrationConfiguration());
            modelBuilder.ApplyConfiguration(new MillenniumNewStockProcessConfiguration());
            modelBuilder.ApplyConfiguration(new MillenniumNewPricesProcessConfiguration());
            modelBuilder.ApplyConfiguration(new MillenniumProductPriceIntegrationConfiguration());
            modelBuilder.ApplyConfiguration(new MillenniumNewProductProcessConfiguration());
            modelBuilder.ApplyConfiguration(new MillenniumProductIntegrationConfiguration());
            modelBuilder.ApplyConfiguration(new ShopifyProductIntegrationConfiguration());
            modelBuilder.ApplyConfiguration(new ShopifyProductPriceIntegrationConfiguration());
            modelBuilder.ApplyConfiguration(new ShopifyListOrderProcessConfiguration());
            modelBuilder.ApplyConfiguration(new ShopifyListOrderIntegrationConfiguration());
            modelBuilder.ApplyConfiguration(new ShopifyUpdateOrderTagNumberProcessConfiguration());
            modelBuilder.ApplyConfiguration(new MillenniumUpdateOrderProcessConfiguration());
            modelBuilder.ApplyConfiguration(new MillenniumListProductManualProcessConfiguration());
            modelBuilder.ApplyConfiguration(new MillenniumProductImageIntegrationConfiguration());
            modelBuilder.ApplyConfiguration(new MillenniumImageIntegrationConfiguration());
            modelBuilder.ApplyConfiguration(new ShopifyProductImageIntegrationConfiguration());
            modelBuilder.ApplyConfiguration(new OmieUpdateOrderProcessConfiguration());
            modelBuilder.ApplyConfiguration(new ParamConfiguration());
            modelBuilder.ApplyConfiguration(new LogsAbandonMessageConfiguration());
            modelBuilder.ApplyConfiguration(new IntegrationErrorConfiguration());
            modelBuilder.ApplyConfiguration(new IntegrationCacheConfiguration());
            modelBuilder.ApplyConfiguration(new UserProfileConfiguration());
            modelBuilder.ApplyConfiguration(new MillenniumOrderStatusUpdateConfiguration());
            modelBuilder.ApplyConfiguration(new MercadoPagoConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}
