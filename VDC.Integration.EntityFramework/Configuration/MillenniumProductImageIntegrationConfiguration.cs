using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VDC.Integration.Domain.Entities.Database.Integrations.Millenium;

namespace VDC.Integration.EntityFramework.Configuration
{
    public class MillenniumProductImageIntegrationConfiguration : IEntityTypeConfiguration<MillenniumProductImageIntegration>
    {
        public void Configure(EntityTypeBuilder<MillenniumProductImageIntegration> builder)
        {
            builder.HasKey(x => new { x.Id });
        }
    }
}
