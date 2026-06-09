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

        // Widen SupabaseId to nullable for unclaimed voice-booking users
        builder.Property(u => u.SupabaseId).IsRequired(false);
        builder.HasIndex(u => u.SupabaseId).IsUnique().HasFilter("\"SupabaseId\" IS NOT NULL");

        // Voice-booking status
        builder.Property(u => u.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(Domain.Enums.UserStatus.Claimed);

        // Index for Vapi caller lookup by phone
        builder.HasIndex(u => u.Phone).HasFilter("\"Phone\" IS NOT NULL");
    }
}
