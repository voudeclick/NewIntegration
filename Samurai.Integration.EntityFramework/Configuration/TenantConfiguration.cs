using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.Domain.Entities.Database.TenantData;

namespace Samurai.Integration.EntityFramework.Configuration
{
    public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
    {
        public void Configure(EntityTypeBuilder<Tenant> builder)
        {
            builder.HasKey(x => new { x.Id });
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.CreationDate).IsRequired(true);
            builder.Property(x => x.UpdateDate).IsRequired(true);
            builder.Property(x => x.StoreName).IsRequired(true);
            builder.Property(x => x.StoreHandle).IsRequired(true);

            builder.HasOne(x => x.ShopifyData)
                       .WithOne()
                       .HasForeignKey<ShopifyData>(x => x.Id)
                       .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.SellerCenterData)
                    .WithOne()
                    .HasForeignKey<SellerCenterData>(x => x.Id)
                    .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.MillenniumData)
                        .WithOne()
                        .HasForeignKey<MillenniumData>(x => x.Id)
                        .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.NexaasData)
                        .WithOne()
                        .HasForeignKey<NexaasData>(x => x.Id)
                        .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.OmieData)
                        .WithOne()
                        .HasForeignKey<OmieData>(x => x.Id)
                        .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.LocationMap)
                    .WithOne()
                    .HasForeignKey<LocationMap>(x => x.Id)
                    .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Pier8Data)
                       .WithOne()
                       .HasForeignKey<Pier8Data>(x => x.Id)
                       .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.BlingData)
                        .WithOne()
                        .HasForeignKey<BlingData>(x => x.Id)
                        .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.PluggToData)
                       .WithOne()
                       .HasForeignKey<PluggToData>(x => x.Id)
                       .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
