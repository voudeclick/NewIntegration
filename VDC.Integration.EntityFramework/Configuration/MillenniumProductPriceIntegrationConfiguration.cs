using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VDC.Integration.Domain.Entities.Database.Integrations.Millenium;

namespace VDC.Integration.EntityFramework.Configuration
{
    public class MillenniumProductPriceIntegrationConfiguration : IEntityTypeConfiguration<MillenniumProductPriceIntegration>
    {
        public void Configure(EntityTypeBuilder<MillenniumProductPriceIntegration> builder)
        {
            builder.HasKey(x => new { x.Id });
        }
    }
}
