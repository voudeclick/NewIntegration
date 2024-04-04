using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VDC.Integration.Domain.Entities.Database.Integrations.Shopify;

namespace VDC.Integration.EntityFramework.Configuration
{
    public class ShopifyProductStockIntegrationConfiguration : IEntityTypeConfiguration<ShopifyProductStockIntegration>
    {
        public void Configure(EntityTypeBuilder<ShopifyProductStockIntegration> builder)
        {
            builder.HasKey(x => new { x.Id });
        }
    }
}
