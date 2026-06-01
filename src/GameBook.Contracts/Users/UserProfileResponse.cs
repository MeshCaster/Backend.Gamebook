namespace GameBook.Contracts.Users;

public sealed record UserProfileResponse(
    Guid Id,
    string DisplayName,
    string? AvatarUrl,
    string? Email,
    string? GamerTag,
    decimal WalletBalance,
    int TotalBookings,
    int TotalReviews);

public sealed record UpdateProfileRequest(
    string? DisplayName,
    string? GamerTag,
    string? AvatarUrl);
