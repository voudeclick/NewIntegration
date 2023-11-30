using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Samurai.Integration.Domain.Entities.Database;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.EntityFramework.Configuration
{
    public class MillenniumNewPricesProcessConfiguration : IEntityTypeConfiguration<MillenniumNewPricesProcess>
    {
        public void Configure(EntityTypeBuilder<MillenniumNewPricesProcess> builder)
        {
            builder.HasKey(x => new { x.Id });
        }
    }
}
