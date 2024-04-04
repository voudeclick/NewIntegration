using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VDC.Integration.Domain.Entities.Database;

namespace VDC.Integration.EntityFramework.Configuration
{
    public class MillenniumOrderStatusUpdateConfiguration : IEntityTypeConfiguration<MillenniumOrderStatusUpdate>
    {
        public void Configure(EntityTypeBuilder<MillenniumOrderStatusUpdate> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }
}
