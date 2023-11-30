using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Samurai.Integration.Domain.Entities.Database.TenantData;
using Samurai.Integration.Domain.Enums;

namespace Samurai.Integration.EntityFramework.Configuration
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