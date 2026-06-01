using GameBook.Contracts.Bookings;
using MediatR;

namespace GameBook.Application.Bookings.Queries.GetUserBookings;

public sealed record GetUserBookingsQuery(Guid UserId) : IRequest<List<BookingResponse>>;
