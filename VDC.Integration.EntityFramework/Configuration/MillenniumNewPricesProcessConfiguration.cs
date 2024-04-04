using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VDC.Integration.Domain.Entities.Database;

namespace VDC.Integration.EntityFramework.Configuration
{
    public class MillenniumNewPricesProcessConfiguration : IEntityTypeConfiguration<MillenniumNewPricesProcess>
    {
        public void Configure(EntityTypeBuilder<MillenniumNewPricesProcess> builder)
        {
            builder.HasKey(x => new { x.Id });
        }
    }
}
