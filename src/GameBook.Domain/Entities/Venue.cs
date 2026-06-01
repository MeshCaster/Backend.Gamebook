using NetTopologySuite.Geometries;

namespace GameBook.Domain.Entities;

public class Venue
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? CoverUrl { get; set; }
    public Point? Location { get; set; }
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public TimeOnly OpensAt { get; set; }
    public TimeOnly ClosesAt { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string[] Amenities { get; set; } = [];
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<Station> Stations { get; set; } = new List<Station>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
