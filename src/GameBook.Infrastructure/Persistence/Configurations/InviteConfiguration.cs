using GameBook.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameBook.Infrastructure.Persistence.Configurations;

public class InviteConfiguration : IEntityTypeConfiguration<Invite>
{
    public void Configure(EntityTypeBuilder<Invite> builder)
    {
        builder.ToTable("invites");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasOne(i => i.Booking).WithMany(b => b.Invites).HasForeignKey(i => i.BookingId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(i => new { i.BookingId, i.InviteeId }).IsUnique();
    }
}
