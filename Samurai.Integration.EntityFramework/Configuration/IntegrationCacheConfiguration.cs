using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Caching.SqlServer;
using Microsoft.Extensions.Options;
using Samurai.Integration.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.EntityFramework.Configuration
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
