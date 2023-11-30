using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Samurai.Integration.Domain.Entities;

namespace Samurai.Integration.EntityFramework.Configuration
{
    public class MercadoPagoConfiguration : IEntityTypeConfiguration<MercadoPago>
    {
        public void Configure(EntityTypeBuilder<MercadoPago> builder)
        {
            builder.HasKey(x => new { x.Id });
            builder.Property(x => x.Authorization).IsRequired();

            builder.HasOne(s=> s.MillenniumData).WithOne(s=> s.MercadoPago);
        }
    }
}
