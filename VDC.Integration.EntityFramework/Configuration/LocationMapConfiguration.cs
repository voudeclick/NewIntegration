using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VDC.Integration.Domain.Entities.Database.TenantData;

namespace VDC.Integration.EntityFramework.Configuration
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