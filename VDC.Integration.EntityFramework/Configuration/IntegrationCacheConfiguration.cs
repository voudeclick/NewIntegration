using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VDC.Integration.Domain.Models;

namespace VDC.Integration.EntityFramework.Configuration
{
    public class IntegrationCacheConfiguration : IEntityTypeConfiguration<IntegrationCache>
    {
        public void Configure(EntityTypeBuilder<IntegrationCache> builder)
        {
            builder.ToTable(name: "IntegrationCache", schema: "dbo");

            builder.HasIndex(e => e.ExpiresAtTime);

            builder.Property(e => e.Id)
                .IsRequired()
                .HasMaxLength(449);

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Value).IsRequired();
        }
    }
}
