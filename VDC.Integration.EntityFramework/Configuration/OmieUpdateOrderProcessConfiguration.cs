using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VDC.Integration.Domain.Entities.Database.Integrations.Omie;

namespace VDC.Integration.EntityFramework.Configuration
{
    public class OmieUpdateOrderProcessConfiguration : IEntityTypeConfiguration<OmieUpdateOrderProcess>
    {
        public void Configure(EntityTypeBuilder<OmieUpdateOrderProcess> builder)
        {
            builder.HasKey(x => new { x.Id });
        }
    }
}
