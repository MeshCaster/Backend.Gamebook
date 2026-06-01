using GameBook.Domain.Enums;
using GameBook.Domain.ValueObjects;

namespace GameBook.Domain.Entities;

public class Booking
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid VenueId { get; set; }
    public Guid StationId { get; set; }
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }
    public int GuestCount { get; set; } = 1;
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; } = "GEL";
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public QrRef QrCode { get; set; }
    public string? CancellationReason { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public Venue Venue { get; set; } = null!;
    public Station Station { get; set; } = null!;
    public Payment? Payment { get; set; }
    public ICollection<Invite> Invites { get; set; } = new List<Invite>();
}
