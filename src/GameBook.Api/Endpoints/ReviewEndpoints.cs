using GameBook.Application.Common.Interfaces;
using Meshcaster.IdentityProvider.Abstractions;
using GameBook.Contracts.Reviews;
using GameBook.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameBook.Api.Endpoints;

public static class ReviewEndpoints
{
    public static RouteGroupBuilder MapReviewEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/v1/reviews").WithTags("Reviews");

        group.MapGet("/venue/{venueSlug}", async (string venueSlug, IGameBookDbContext db) =>
        {
            var venue = await db.Venues.AsNoTracking().FirstOrDefaultAsync(v => v.Slug == venueSlug);
            if (venue is null) return Results.NotFound();

            var reviews = await db.Reviews
                .Include(r => r.User)
                .Where(r => r.VenueId == venue.Id)
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking()
                .Select(r => new ReviewResponse(
                    r.Id, r.UserId, r.User.DisplayName, r.User.AvatarUrl,
                    r.Rating, r.Comment, r.CreatedAt))
                .ToListAsync();

            return Results.Ok(reviews);
        }).WithName("GetVenueReviews");

        group.MapPost("/", async (CreateReviewRequest request, ICurrentUser user, IGameBookDbContext db) =>
        {
            var review = new Review
            {
                Id = Guid.NewGuid(),
                UserId = user.UserId,
                VenueId = request.VenueId,
                BookingId = request.BookingId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTimeOffset.UtcNow
            };

            db.Reviews.Add(review);
            await db.SaveChangesAsync();
            return Results.Created($"/v1/reviews/{review.Id}", review);
        }).RequireAuthorization().WithName("CreateReview");

        return group;
    }
}
