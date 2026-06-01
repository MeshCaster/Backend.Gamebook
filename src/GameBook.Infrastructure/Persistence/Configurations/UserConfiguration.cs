using GameBook.Domain.Entities;
using Meshcaster.IdentityProvider.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameBook.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IdentityUserBaseConfiguration<User>
{
    protected override void ConfigureAdditional(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.GamerTag).HasMaxLength(50);
        builder.HasIndex(u => u.GamerTag).IsUnique().HasFilter("\"GamerTag\" IS NOT NULL");
    }
}
