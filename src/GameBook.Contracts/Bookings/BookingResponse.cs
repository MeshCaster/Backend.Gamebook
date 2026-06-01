namespace GameBook.Contracts.Bookings;

public sealed record BookingResponse(
    Guid Id,
    Guid VenueId,
    string VenueName,
    string VenueSlug,
    Guid StationId,
    string StationLabel,
    string StationKind,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    int GuestCount,
    decimal TotalPrice,
    string Currency,
    string Status,
    string QrCode,
    DateTimeOffset CreatedAt);
