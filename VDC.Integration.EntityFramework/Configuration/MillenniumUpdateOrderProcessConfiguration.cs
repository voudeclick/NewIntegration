using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VDC.Integration.Domain.Entities.Database.Integrations.Millenium;

namespace VDC.Integration.EntityFramework.Configuration
{
    public class MillenniumUpdateOrderProcessConfiguration : IEntityTypeConfiguration<MillenniumUpdateOrderProcess>
    {
        public void Configure(EntityTypeBuilder<MillenniumUpdateOrderProcess> builder)
        {
            builder.HasKey(x => new { x.Id });
        }
    }
}
