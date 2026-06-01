namespace GameBook.Contracts.Bookings;

public sealed record CreateBookingRequest(
    Guid VenueId,
    Guid StationId,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    int GuestCount);
