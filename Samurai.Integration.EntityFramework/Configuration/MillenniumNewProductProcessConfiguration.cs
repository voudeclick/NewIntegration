using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Samurai.Integration.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.EntityFramework.Configuration
{
    public class MillenniumNewProductProcessConfiguration : IEntityTypeConfiguration<MillenniumNewProductProcess>
    {
        public void Configure(EntityTypeBuilder<MillenniumNewProductProcess> builder)
        {
            builder.HasKey(x => new { x.Id });
        }
    }
}
