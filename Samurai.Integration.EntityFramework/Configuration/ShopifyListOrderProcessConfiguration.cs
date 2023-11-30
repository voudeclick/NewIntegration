using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.Domain.Entities.Database.Integrations.Shopify;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.EntityFramework.Configuration
{
    public class ShopifyListOrderProcessConfiguration : IEntityTypeConfiguration<ShopifyListOrderProcess>
    {
        public void Configure(EntityTypeBuilder<ShopifyListOrderProcess> builder)
        {
            builder.HasKey(x => new { x.Id });
        }
    }
}
