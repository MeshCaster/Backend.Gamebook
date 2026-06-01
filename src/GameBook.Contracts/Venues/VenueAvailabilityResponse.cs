namespace GameBook.Contracts.Venues;

public sealed record VenueAvailabilityResponse(
    Guid VenueId,
    DateOnly Date,
    List<StationAvailability> Stations);

public sealed record StationAvailability(
    Guid StationId,
    string Label,
    string Kind,
    decimal PricePerHour,
    List<AvailableWindow> AvailableWindows);

public sealed record AvailableWindow(
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt);
