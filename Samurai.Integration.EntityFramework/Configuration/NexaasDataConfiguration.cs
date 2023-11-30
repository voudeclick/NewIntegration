using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Samurai.Integration.Domain.Entities.Database.TenantData;

namespace Samurai.Integration.EntityFramework.Configuration
{
    public class NexaasDataConfiguration : IEntityTypeConfiguration<NexaasData>
    {
        public void Configure(EntityTypeBuilder<NexaasData> builder)
        {
            builder.HasKey(x => new { x.Id });
            builder.Property(x => x.CreationDate).IsRequired(true);
            builder.Property(x => x.UpdateDate).IsRequired(true);            
            builder.Property(x => x.Url).IsRequired(true);
            builder.Property(x => x.Token).IsRequired(true);
            builder.Property(x => x.OrganizationId).IsRequired(true);
            builder.Property(x => x.SaleChannelId).IsRequired(true);
            builder.Property(x => x.StockId).IsRequired(true);
            builder.Property(x => x.OrderPrefix).IsRequired(false);
        }
    }
}
