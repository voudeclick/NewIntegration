using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VDC.Integration.Domain.Entities.Database.Integrations.Millenium;

namespace VDC.Integration.EntityFramework.Configuration
{
    public class MillenniumProductStockIntegrationConfiguration : IEntityTypeConfiguration<MillenniumProductStockIntegration>
    {
        public void Configure(EntityTypeBuilder<MillenniumProductStockIntegration> builder)
        {
            builder.HasKey(x => new { x.Id });
        }
    }
}
