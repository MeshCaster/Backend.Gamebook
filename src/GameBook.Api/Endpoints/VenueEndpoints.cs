using GameBook.Application.Venues.Queries.GetVenueAvailability;
using GameBook.Application.Venues.Queries.GetVenueBySlug;
using GameBook.Application.Venues.Queries.GetVenues;
using MediatR;

namespace GameBook.Api.Endpoints;

public static class VenueEndpoints
{
    public static RouteGroupBuilder MapVenueEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/v1/venues").WithTags("Venues");

        group.MapGet("/", async (string? search, double? lat, double? lng, ISender sender) =>
        {
            var result = await sender.Send(new GetVenuesQuery(search, lat, lng));
            return Results.Ok(result);
        }).WithName("GetVenues");

        group.MapGet("/{slug}", async (string slug, ISender sender) =>
        {
            var result = await sender.Send(new GetVenueBySlugQuery(slug));
            return result is null ? Results.NotFound() : Results.Ok(result);
        }).WithName("GetVenueBySlug");

        group.MapGet("/{slug}/availability", async (string slug, DateOnly? date, ISender sender) =>
        {
            var queryDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var result = await sender.Send(new GetVenueAvailabilityQuery(slug, queryDate));
            return result is null ? Results.NotFound() : Results.Ok(result);
        }).WithName("GetVenueAvailability");

        return group;
    }
}
