using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Samurai.Integration.Domain.Entities.Database.TenantData;

namespace Samurai.Integration.EntityFramework.Configuration
{
    public class LocationMapConfiguration : IEntityTypeConfiguration<LocationMap>
    {
        public void Configure(EntityTypeBuilder<LocationMap> builder)
        {
            builder.HasKey(x => new { x.Id });
            builder.Property(x => x.CreationDate).IsRequired(true);
            builder.Property(x => x.JsonMap).IsRequired(true);
        }
    }
}