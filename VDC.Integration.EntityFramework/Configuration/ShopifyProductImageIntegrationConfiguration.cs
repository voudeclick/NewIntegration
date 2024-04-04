using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VDC.Integration.Domain.Entities.Database.Integrations.Shopify;

namespace VDC.Integration.EntityFramework.Configuration
{
    public class ShopifyProductImageIntegrationConfiguration : IEntityTypeConfiguration<ShopifyProductImageIntegration>
    {
        public void Configure(EntityTypeBuilder<ShopifyProductImageIntegration> builder)
        {
            builder.HasKey(x => new { x.Id });
        }
    }
}
