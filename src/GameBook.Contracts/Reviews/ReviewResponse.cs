namespace GameBook.Contracts.Reviews;

public sealed record ReviewResponse(
    Guid Id,
    Guid UserId,
    string UserDisplayName,
    string? UserAvatarUrl,
    int Rating,
    string? Comment,
    DateTimeOffset CreatedAt);

public sealed record CreateReviewRequest(
    Guid VenueId,
    Guid? BookingId,
    int Rating,
    string? Comment);
