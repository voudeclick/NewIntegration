using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VDC.Integration.Domain.Entities.Database.Integrations.Shopify;

namespace VDC.Integration.EntityFramework.Configuration
{
    public class ShopifyProductPriceIntegrationConfiguration : IEntityTypeConfiguration<ShopifyProductPriceIntegration>
    {
        public void Configure(EntityTypeBuilder<ShopifyProductPriceIntegration> builder)
        {
            builder.HasKey(x => new { x.Id });
        }
    }
}
