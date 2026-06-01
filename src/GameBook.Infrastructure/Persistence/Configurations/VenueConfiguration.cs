using GameBook.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameBook.Infrastructure.Persistence.Configurations;

public class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.ToTable("venues");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(v => v.Name).HasMaxLength(200);
        builder.Property(v => v.Slug).HasMaxLength(200);
        builder.HasIndex(v => v.Slug).IsUnique();
        builder.Property(v => v.Description).HasMaxLength(2000);
        builder.Property(v => v.Address).HasMaxLength(500);
        builder.Property(v => v.Location).HasColumnType("geometry(Point, 4326)");
        builder.Property(v => v.Phone).HasMaxLength(20);
        builder.Property(v => v.Website).HasMaxLength(500);
        builder.HasIndex(v => v.IsActive);
        builder.HasIndex(v => v.IsFeatured).HasFilter("\"IsFeatured\" = true");
    }
}
