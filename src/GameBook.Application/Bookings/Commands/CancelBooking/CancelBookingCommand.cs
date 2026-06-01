using MediatR;

namespace GameBook.Application.Bookings.Commands.CancelBooking;

public sealed record CancelBookingCommand(Guid BookingId, Guid UserId, string? Reason) : IRequest<bool>;
