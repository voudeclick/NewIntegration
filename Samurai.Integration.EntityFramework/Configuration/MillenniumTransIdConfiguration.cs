using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Samurai.Integration.Domain.Entities.Database.TenantData;

namespace Samurai.Integration.EntityFramework.Configuration
{
    public class MillenniumTransIdConfiguration : IEntityTypeConfiguration<MillenniumTransId>
    {
        public void Configure(EntityTypeBuilder<MillenniumTransId> builder)
        {
            builder.HasKey(x => new { x.Id });
        }
    }
}
