using GameBook.Contracts.Bookings;
using MediatR;

namespace GameBook.Application.Bookings.Queries.GetBookingById;

public sealed record GetBookingByIdQuery(Guid BookingId, Guid UserId) : IRequest<BookingResponse?>;
