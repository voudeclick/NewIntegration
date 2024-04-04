using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VDC.Integration.Domain.Entities.Database.Integrations.Shopify;

namespace VDC.Integration.EntityFramework.Configuration
{
    public class ShopifyListOrderIntegrationConfiguration : IEntityTypeConfiguration<ShopifyListOrderIntegration>
    {
        public void Configure(EntityTypeBuilder<ShopifyListOrderIntegration> builder)
        {
            builder.HasKey(x => new { x.Id });
        }
    }
}
