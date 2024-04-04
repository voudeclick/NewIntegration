using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VDC.Integration.Domain.Entities.Database;
using VDC.Integration.Domain.Enums;

namespace VDC.Integration.EntityFramework.Configuration
{
    public class IntegrationErrorConfiguration : IEntityTypeConfiguration<IntegrationError>
    {
        public void Configure(EntityTypeBuilder<IntegrationError> builder)
        {
            builder.HasKey(x => x.Tag);
            builder.Property(x => x.Tag).HasMaxLength(50);
            builder.Property(x => x.Message).HasMaxLength(200);
            builder.Property(x => x.MessagePattern).HasMaxLength(200);
            builder.Property(x => x.Description).HasMaxLength(400);

            builder.Property(x => x.SourceId)
                .HasConversion(new EnumToStringConverter<IntegrationErrorSource>());
        }
    }
}
