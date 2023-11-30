using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Samurai.Integration.Domain.Entities.Database.Integrations.Omie;

namespace Samurai.Integration.EntityFramework.Configuration
{
    public class OmieUpdateOrderProcessConfiguration : IEntityTypeConfiguration<OmieUpdateOrderProcess>
    {
        public void Configure(EntityTypeBuilder<OmieUpdateOrderProcess> builder)
        {
            builder.HasKey(x => new { x.Id });            
        }
    }
}
