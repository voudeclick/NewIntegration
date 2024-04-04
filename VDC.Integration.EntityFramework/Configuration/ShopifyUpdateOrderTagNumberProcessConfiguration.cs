using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VDC.Integration.Domain.Entities.Database.Integrations.Shopify;

namespace VDC.Integration.EntityFramework.Configuration
{
    public class ShopifyUpdateOrderTagNumberProcessConfiguration : IEntityTypeConfiguration<ShopifyUpdateOrderTagNumberProcess>
    {
        public void Configure(EntityTypeBuilder<ShopifyUpdateOrderTagNumberProcess> builder)
        {
            builder.HasKey(x => new { x.Id });
        }
    }
}
