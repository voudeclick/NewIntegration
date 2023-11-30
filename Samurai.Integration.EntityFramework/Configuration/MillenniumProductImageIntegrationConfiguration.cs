using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Samurai.Integration.Domain.Entities.Database.Integrations.Millenium;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.EntityFramework.Configuration
{
    public class MillenniumProductImageIntegrationConfiguration : IEntityTypeConfiguration<MillenniumProductImageIntegration>
    {
        public void Configure(EntityTypeBuilder<MillenniumProductImageIntegration> builder)
        {
            builder.HasKey(x => new { x.Id });
        }
    }
}
