using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.Domain.ValueObjects;
using Samurai.Integration.EntityFramework.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.EntityFramework.Configuration
{
    public class ParamConfiguration : IEntityTypeConfiguration<Param>
    {
        public void Configure(EntityTypeBuilder<Param> builder)
        {
            builder.HasKey(x => x.Key);

            builder.Property(x => x.Values).HasJsonConversion();
        }
    }
}
