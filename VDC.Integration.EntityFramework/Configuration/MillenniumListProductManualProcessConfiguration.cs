using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VDC.Integration.Domain.Entities.Database;

namespace VDC.Integration.EntityFramework.Configuration
{
    public class MillenniumListProductManualProcessConfiguration : IEntityTypeConfiguration<MillenniumListProductManualProcess>
    {
        public void Configure(EntityTypeBuilder<MillenniumListProductManualProcess> builder)
        {
            builder.HasKey(x => new { x.Id });
        }
    }
}
