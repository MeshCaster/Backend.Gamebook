namespace GameBook.Contracts.Venues;

public sealed record VenueResponse(
    Guid Id,
    string Name,
    string Slug,
    string Description,
    string Address,
    string? ImageUrl,
    string? CoverUrl,
    double? Latitude,
    double? Longitude,
    double Rating,
    int ReviewCount,
    string OpensAt,
    string ClosesAt,
    string? Phone,
    string? Website,
    string[] Amenities,
    List<StationResponse> Stations,
    bool IsFeatured = false,
    double? DistanceKm = null);

public sealed record StationResponse(
    Guid Id,
    string Label,
    string Kind,
    decimal PricePerHour,
    int Capacity,
    string[] Specs);

public sealed record VenueListResponse(
    List<VenueResponse> Venues,
    int Total);
