using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Samurai.Integration.Domain.Entities.Database.TenantData;
using Samurai.Integration.Domain.Enums.Millennium;

namespace Samurai.Integration.EntityFramework.Configuration
{
    public class ShopifyDataConfiguration : IEntityTypeConfiguration<ShopifyData>
    {
         
        public void Configure(EntityTypeBuilder<ShopifyData> builder)
        {
            builder.HasKey(x => new { x.Id });
            builder.Property(x => x.CreationDate).IsRequired(true);
            builder.Property(x => x.UpdateDate).IsRequired(true);
            builder.Property(x => x.ShopifyStoreDomain).IsRequired(true);
            builder.Property(x => x.ShopifyAppJson).IsRequired(true);
            builder.Property(x => x.DaysToDelivery).IsRequired(true);
            builder.Property(x => x.MinOrderId).IsRequired(true);
            builder.Property(x => x.ShipmentServicesForFulfillmentNotification).HasMaxLength(256);
            builder.Property(x => x.SkuFieldType).HasConversion(new EnumToStringConverter<SkuFieldType>());
        }
    
    }
}
