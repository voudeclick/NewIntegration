using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Samurai.Integration.Domain.Entities.Database.TenantData;

namespace Samurai.Integration.EntityFramework.Configuration
{
    public class Pier8DataConfiguration : IEntityTypeConfiguration<Pier8Data>
    {
        public void Configure(EntityTypeBuilder<Pier8Data> builder)
        {
            builder.HasKey(x => new { x.Id });
            builder.Property(x => x.CreationDate).IsRequired(true);
            builder.Property(x => x.ApiKey).IsRequired(true);
            builder.Property(x => x.Token).IsRequired(true);

        }
    }
}
