using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Samurai.Integration.Domain.Entities.Database;

namespace Samurai.Integration.EntityFramework.Configuration
{
    public class MillenniumOrderStatusUpdateConfiguration : IEntityTypeConfiguration<MillenniumOrderStatusUpdate>
    {
        public void Configure(EntityTypeBuilder<MillenniumOrderStatusUpdate> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }
}
