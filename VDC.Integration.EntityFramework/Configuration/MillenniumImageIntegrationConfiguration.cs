using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VDC.Integration.Domain.Entities.Database.Integrations.Millenium;

namespace VDC.Integration.EntityFramework.Configuration
{
    public class MillenniumImageIntegrationConfiguration : IEntityTypeConfiguration<MillenniumImageIntegration>
    {
        public void Configure(EntityTypeBuilder<MillenniumImageIntegration> builder)
        {
            builder.HasKey(x => new { x.Id });
        }
    }
}
