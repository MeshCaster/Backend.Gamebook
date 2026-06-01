using GameBook.Application.Bookings.Commands.CancelBooking;
using GameBook.Application.Bookings.Commands.CreateBooking;
using GameBook.Application.Bookings.Queries.GetBookingById;
using GameBook.Application.Bookings.Queries.GetUserBookings;
using GameBook.Contracts.Bookings;
using Meshcaster.IdentityProvider.Abstractions;
using MediatR;

namespace GameBook.Api.Endpoints;

public static class BookingEndpoints
{
    public static RouteGroupBuilder MapBookingEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/v1/bookings").WithTags("Bookings").RequireAuthorization();

        group.MapGet("/", async (ICurrentUser user, ISender sender) =>
        {
            var result = await sender.Send(new GetUserBookingsQuery(user.UserId));
            return Results.Ok(result);
        }).WithName("GetMyBookings");

        group.MapGet("/{id:guid}", async (Guid id, ICurrentUser user, ISender sender) =>
        {
            var result = await sender.Send(new GetBookingByIdQuery(id, user.UserId));
            return result is not null ? Results.Ok(result) : Results.NotFound();
        }).WithName("GetBookingById");

        group.MapPost("/", async (CreateBookingRequest request, ICurrentUser user, ISender sender) =>
        {
            var command = new CreateBookingCommand(
                user.UserId, request.VenueId, request.StationId,
                request.StartsAt, request.EndsAt, request.GuestCount);
            var result = await sender.Send(command);
            return Results.Created($"/v1/bookings/{result.Id}", result);
        }).WithName("CreateBooking");

        group.MapPost("/{id:guid}/cancel", async (Guid id, ICurrentUser user, ISender sender) =>
        {
            var result = await sender.Send(new CancelBookingCommand(id, user.UserId, null));
            return result ? Results.Ok() : Results.NotFound();
        }).WithName("CancelBooking");

        return group;
    }
}
