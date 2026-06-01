using GameBook.Contracts.Bookings;
using MediatR;

namespace GameBook.Application.Bookings.Commands.CreateBooking;

public sealed record CreateBookingCommand(
    Guid UserId,
    Guid VenueId,
    Guid StationId,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    int GuestCount) : IRequest<BookingResponse>;
