using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VDC.Integration.Domain.Entities.Database;

namespace VDC.Integration.EntityFramework.Configuration
{
    public class MillenniumNewStockProcessConfiguration : IEntityTypeConfiguration<MillenniumNewStockProcess>
    {
        public void Configure(EntityTypeBuilder<MillenniumNewStockProcess> builder)
        {
            builder.HasKey(x => new { x.Id });
        }
    }
}
