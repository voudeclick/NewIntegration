using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Samurai.Integration.Domain.Entities.Database;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.EntityFramework.Configuration
{
    public class MillenniumNewStockProcessConfiguration : IEntityTypeConfiguration<MillenniumNewStockProcess>
    {
        public void Configure(EntityTypeBuilder<MillenniumNewStockProcess> builder)
        {
            builder.HasKey(x => new { x.Id });
        }
    }
}
