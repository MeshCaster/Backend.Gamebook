using GameBook.Domain.Entities;
using GameBook.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameBook.Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(b => b.TotalPrice).HasColumnType("decimal(10,2)");
        builder.Property(b => b.Currency).HasMaxLength(10).HasDefaultValue("GEL");
        builder.Property(b => b.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(b => b.QrCode).HasConversion(
            q => q.Value,
            v => new QrRef(v)).HasMaxLength(64);
        builder.Property(b => b.CancellationReason).HasMaxLength(500);

        builder.HasOne(b => b.User).WithMany(u => u.Bookings).HasForeignKey(b => b.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(b => b.Venue).WithMany(v => v.Bookings).HasForeignKey(b => b.VenueId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(b => b.Station).WithMany(s => s.Bookings).HasForeignKey(b => b.StationId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => new { b.StationId, b.StartsAt, b.EndsAt });
        builder.HasIndex(b => b.UserId);
        builder.HasIndex(b => b.Status);
    }
}
