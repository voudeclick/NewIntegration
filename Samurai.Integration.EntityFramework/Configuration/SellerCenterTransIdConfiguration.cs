using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Samurai.Integration.Domain.Entities.Database.TenantData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.EntityFramework.Configuration
{
    public class SellerCenterTransIdConfiguration : IEntityTypeConfiguration<SellerCenterTransId>
    {
        public void Configure(EntityTypeBuilder<SellerCenterTransId> builder)
        {
            builder.HasKey(x => new { x.Id });
        }
    }
}
