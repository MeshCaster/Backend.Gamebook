using GameBook.Application.Common.Interfaces;
using GameBook.Domain.Entities;
using GameBook.Infrastructure.Persistence;
using Meshcaster.IdentityProvider.Abstractions;
using Meshcaster.IdentityProvider.Services;
using GameBook.Contracts.Users;
using GameBook.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GameBook.Api.Endpoints;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/v1/users").WithTags("Users").RequireAuthorization();

        group.MapGet("/me", async (ICurrentUser currentUser, IGameBookDbContext db) =>
        {
            var user = await db.Users
                .Include(u => u.Wallet)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == currentUser.UserId);

            if (user is null) return Results.NotFound();

            var totalBookings = await db.Bookings.CountAsync(b => b.UserId == user.Id);
            var totalReviews = await db.Reviews.CountAsync(r => r.UserId == user.Id);

            return Results.Ok(new UserProfileResponse(
                user.Id, user.DisplayName, user.AvatarUrl, user.Email,
                user.GamerTag, user.Wallet?.Balance ?? 0m, totalBookings, totalReviews));
        }).WithName("GetMyProfile");

        group.MapPut("/me", async (UpdateProfileRequest request, ICurrentUser currentUser, IGameBookDbContext db) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == currentUser.UserId);
            if (user is null) return Results.NotFound();

            if (request.DisplayName is not null) user.DisplayName = request.DisplayName;
            if (request.GamerTag is not null) user.GamerTag = request.GamerTag;
            if (request.AvatarUrl is not null) user.AvatarUrl = request.AvatarUrl;
            user.UpdatedAt = DateTimeOffset.UtcNow;

            await db.SaveChangesAsync();
            return Results.Ok();
        }).WithName("UpdateMyProfile");

        group.MapDelete("/me", async (ICurrentUser currentUser,
            UserDeletionService<User, GameBookDbContext> deletionService) =>
        {
            var deleted = await deletionService.DeleteBySupabaseIdAsync(currentUser.SupabaseId);
            return deleted ? Results.NoContent() : Results.NotFound();
        }).WithName("DeleteMyAccount");

        return group;
    }
}
