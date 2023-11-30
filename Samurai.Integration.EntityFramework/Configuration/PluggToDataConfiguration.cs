using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Samurai.Integration.Domain.Entities.Database.TenantData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.EntityFramework.Configuration
{
    public class PluggToDataConfiguration : IEntityTypeConfiguration<PluggToData>
    {
        public void Configure(EntityTypeBuilder<PluggToData> builder)
        {
            builder.HasKey(x => new { x.Id });
            builder.Property(x => x.CreationDate).IsRequired(true);
            builder.Property(x => x.UpdateDate).IsRequired(true);

            builder.Property(x => x.ClientId).IsRequired(true);
            builder.Property(x => x.ClientSecret).IsRequired(true);
            builder.Property(x => x.Username).IsRequired(true);
            builder.Property(x => x.Password).IsRequired(true);
            builder.Property(x => x.AccountUserId).IsRequired(true);

        }
    }
}
