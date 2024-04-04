using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VDC.Integration.Domain.Entities.Database.Integrations.Millenium;

namespace VDC.Integration.EntityFramework.Configuration
{
    public class MillenniumProductIntegrationConfiguration : IEntityTypeConfiguration<MillenniumProductIntegration>
    {
        public void Configure(EntityTypeBuilder<MillenniumProductIntegration> builder)
        {
            builder.HasKey(x => new { x.Id });
        }
    }
}
