using GameBook.Domain.Enums;

namespace GameBook.Domain.Entities;

public class Station
{
    public Guid Id { get; set; }
    public Guid VenueId { get; set; }
    public string Label { get; set; } = string.Empty;
    public StationKind Kind { get; set; }
    public decimal PricePerHour { get; set; }
    public int Capacity { get; set; } = 1;
    public string[] Specs { get; set; } = [];
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public Venue Venue { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
