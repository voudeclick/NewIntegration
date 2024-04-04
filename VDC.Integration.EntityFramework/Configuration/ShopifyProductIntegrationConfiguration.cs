using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VDC.Integration.Domain.Entities.Database.Integrations.Shopify;

namespace VDC.Integration.EntityFramework.Configuration
{
    public class ShopifyProductIntegrationConfiguration : IEntityTypeConfiguration<ShopifyProductIntegration>
    {
        public void Configure(EntityTypeBuilder<ShopifyProductIntegration> builder)
        {
            builder.HasKey(x => new { x.Id });
        }
    }
}
