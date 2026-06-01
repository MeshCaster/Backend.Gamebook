using GameBook.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameBook.Infrastructure.Persistence.Configurations;

public class StationConfiguration : IEntityTypeConfiguration<Station>
{
    public void Configure(EntityTypeBuilder<Station> builder)
    {
        builder.ToTable("stations");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(s => s.Label).HasMaxLength(100);
        builder.Property(s => s.Kind).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.PricePerHour).HasColumnType("decimal(10,2)");
        builder.HasOne(s => s.Venue).WithMany(v => v.Stations).HasForeignKey(s => s.VenueId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(s => new { s.VenueId, s.Kind });
    }
}
