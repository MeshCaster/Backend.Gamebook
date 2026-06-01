using GameBook.Application.Common.Interfaces;
using GameBook.Contracts.Bookings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GameBook.Application.Bookings.Queries.GetUserBookings;

public sealed class GetUserBookingsHandler : IRequestHandler<GetUserBookingsQuery, List<BookingResponse>>
{
    private readonly IGameBookDbContext _db;

    public GetUserBookingsHandler(IGameBookDbContext db) => _db = db;

    public async Task<List<BookingResponse>> Handle(GetUserBookingsQuery request, CancellationToken ct)
    {
        var bookings = await _db.Bookings
            .Include(b => b.Venue)
            .Include(b => b.Station)
            .Where(b => b.UserId == request.UserId)
            .OrderByDescending(b => b.StartsAt)
            .AsNoTracking()
            .ToListAsync(ct);

        return bookings.Select(b => new BookingResponse(
            b.Id, b.VenueId, b.Venue.Name, b.Venue.Slug,
            b.StationId, b.Station.Label, b.Station.Kind.ToString(),
            b.StartsAt, b.EndsAt, b.GuestCount,
            b.TotalPrice, b.Currency, b.Status.ToString(),
            b.QrCode.Value, b.CreatedAt
        )).ToList();
    }
}
