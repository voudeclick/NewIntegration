using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Samurai.Integration.Domain.Entities.Database.Integrations.Shopify;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.EntityFramework.Configuration
{
    public class ShopifyProductPriceIntegrationConfiguration : IEntityTypeConfiguration<ShopifyProductPriceIntegration>
    {
        public void Configure(EntityTypeBuilder<ShopifyProductPriceIntegration> builder)
        {
            builder.HasKey(x => new { x.Id });
        }
    }
}
