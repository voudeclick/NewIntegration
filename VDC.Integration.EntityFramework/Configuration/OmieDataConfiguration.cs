using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VDC.Integration.Domain.Entities.Database.TenantData;
using VDC.Integration.Domain.Enums;

namespace VDC.Integration.EntityFramework.Configuration
{
    public class OmieDataConfiguration : IEntityTypeConfiguration<OmieData>
    {
        public void Configure(EntityTypeBuilder<OmieData> builder)
        {
            builder.HasKey(x => new { x.Id });
            builder.Property(x => x.CreationDate).IsRequired(true);
            builder.Property(x => x.UpdateDate).IsRequired(true);
            builder.Property(x => x.AppKey).IsRequired(true);
            builder.Property(x => x.AppSecret).IsRequired(true);
            builder.Property(x => x.NameField).HasConversion(new EnumToStringConverter<NameFieldOmieType>());
            builder.Property(x => x.SendNotaFiscalEmailToClient).IsRequired(true);
        }
    }
}