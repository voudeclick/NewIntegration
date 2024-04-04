using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VDC.Integration.Domain.Entities.Database.Integrations.Shopify;

namespace VDC.Integration.EntityFramework.Configuration
{
    public class ShopifyListOrderProcessConfiguration : IEntityTypeConfiguration<ShopifyListOrderProcess>
    {
        public void Configure(EntityTypeBuilder<ShopifyListOrderProcess> builder)
        {
            builder.HasKey(x => new { x.Id });
        }
    }
}
