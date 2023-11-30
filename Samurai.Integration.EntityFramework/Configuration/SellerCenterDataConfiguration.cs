using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Samurai.Integration.Domain.Entities.Database.TenantData;

namespace Samurai.Integration.EntityFramework.Configuration
{
    public class SellerCenterDataConfiguration : IEntityTypeConfiguration<SellerCenterData>
    {
        public void Configure(EntityTypeBuilder<SellerCenterData> builder)
        {
            builder.HasKey(x => new { x.Id });
            builder.Property(x => x.CreationDate).IsRequired(true);
            builder.Property(x => x.UpdateDate).IsRequired(true);
            builder.Property(x => x.Username).IsRequired(true);
            builder.Property(x => x.Password).IsRequired(true);
            builder.Property(x => x.TenantId).IsRequired(true);
            builder.Property(x => x.SellerId).IsRequired(true);
            builder.Property(x => x.SellerWarehouseId).IsRequired(true);
            builder.Property(x => x.SellWithoutStock).IsRequired(true).HasDefaultValue(false);
            builder.Property(x => x.OrderIntegrationStatus).IsRequired(true);
            builder.HasMany(x => x.TransIds).WithOne().HasForeignKey(x => x.Id).OnDelete(DeleteBehavior.Cascade);
        }
    }

}
