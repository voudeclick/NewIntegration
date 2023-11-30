using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Samurai.Integration.Domain.Entities.Database.TenantData;
using Samurai.Integration.Domain.Enums.Millennium;

namespace Samurai.Integration.EntityFramework.Configuration
{
    public class MillenniumDataConfiguration : IEntityTypeConfiguration<MillenniumData>
    {
        public void Configure(EntityTypeBuilder<MillenniumData> builder)
        {
            builder.HasKey(x => new { x.Id });
            builder.Property(x => x.CreationDate).IsRequired(true);
            builder.Property(x => x.UpdateDate).IsRequired(true);
            builder.Property(x => x.LoginJson).IsRequired(true);
            builder.Property(x => x.Url).IsRequired(true);
            builder.Property(x => x.NameField).IsRequired(true);
            builder.Property(x => x.DescriptionField).IsRequired(true);
            builder.Property(x => x.OperatorType).IsRequired(true);
            builder.Property(x => x.NameSkuEnabled).IsRequired(true);
            builder.Property(x => x.EnabledApprovedTransaction).IsRequired(true);
            builder.Property(x => x.SkuFieldType).HasConversion(new EnumToStringConverter<SkuFieldType>());

            builder.HasMany(x => x.TransIds).WithOne().HasForeignKey(x => x.MillenniumDataId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
