
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Samurai.Integration.Domain.Entities.Database.TenantData;
using Samurai.Integration.Domain.Enums.Bling;

namespace Samurai.Integration.EntityFramework.Configuration
{
    public class BlingDataConfiguration : IEntityTypeConfiguration<BlingData>
    {
        public void Configure(EntityTypeBuilder<BlingData> builder)
        {
            builder.HasKey(x => new { x.Id });
            builder.Property(x => x.CreationDate).IsRequired(true);
            builder.Property(x => x.UpdateDate).IsRequired(true);
            builder.Property(x => x.ApiBaseUrl).IsRequired(true);
            builder.Property(x => x.APIKey).IsRequired(true);
            builder.Property(x => x.OrderField).HasConversion(new EnumToStringConverter<OrderFieldBlingType>());

        }
    }
}
