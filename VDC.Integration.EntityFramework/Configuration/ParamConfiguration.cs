using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VDC.Integration.Domain.Entities.Database;
using VDC.Integration.EntityFramework.Extensions;

namespace VDC.Integration.EntityFramework.Configuration
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
