using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Samurai.Integration.Domain.Entities.Database.Logs;

namespace Samurai.Integration.EntityFramework.Configuration
{
    public class LogsAbandonMessageConfiguration : IEntityTypeConfiguration<LogsAbandonMessage>
    {
        public void Configure(EntityTypeBuilder<LogsAbandonMessage> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.LogId).IsRequired();            
            builder.Property(x => x.Payload).IsRequired();
            builder.Property(x => x.Method).IsRequired();
            builder.Property(x => x.TenantId).IsRequired();
            builder.Property(x => x.Type).IsRequired();
        }
    }
}
