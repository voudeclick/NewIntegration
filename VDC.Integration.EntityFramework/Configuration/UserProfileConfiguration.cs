using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VDC.Integration.Domain.Entities.Database;

namespace VDC.Integration.EntityFramework.Configuration
{
    public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
    {
        public void Configure(EntityTypeBuilder<UserProfile> builder)
        {
            builder.HasKey(x => new { x.Id });
            builder.Property(x => x.Id).HasMaxLength(450);

            builder.Property(x => x.Name).HasMaxLength(256);
        }
    }
}
