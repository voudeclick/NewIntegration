using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VDC.Integration.Domain.Entities.Database.TenantData;

namespace VDC.Integration.EntityFramework.Configuration
{
    public class MillenniumTransIdConfiguration : IEntityTypeConfiguration<MillenniumTransId>
    {
        public void Configure(EntityTypeBuilder<MillenniumTransId> builder)
        {
            builder.HasKey(x => new { x.Id });
        }
    }
}
