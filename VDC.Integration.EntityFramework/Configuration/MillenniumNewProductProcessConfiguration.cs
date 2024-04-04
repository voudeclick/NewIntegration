using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VDC.Integration.Domain.Entities.Database;

namespace VDC.Integration.EntityFramework.Configuration
{
    public class MillenniumNewProductProcessConfiguration : IEntityTypeConfiguration<MillenniumNewProductProcess>
    {
        public void Configure(EntityTypeBuilder<MillenniumNewProductProcess> builder)
        {
            builder.HasKey(x => new { x.Id });
        }
    }
}
