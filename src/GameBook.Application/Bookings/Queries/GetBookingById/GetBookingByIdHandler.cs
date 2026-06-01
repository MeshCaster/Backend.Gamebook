using GameBook.Application.Common.Interfaces;
using GameBook.Contracts.Bookings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GameBook.Application.Bookings.Queries.GetBookingById;

public sealed class GetBookingByIdHandler : IRequestHandler<GetBookingByIdQuery, BookingResponse?>
{
    private readonly IGameBookDbContext _db;

    public GetBookingByIdHandler(IGameBookDbContext db) => _db = db;

    public async Task<BookingResponse?> Handle(GetBookingByIdQuery request, CancellationToken ct)
    {
        var b = await _db.Bookings
            .Include(b => b.Venue)
            .Include(b => b.Station)
            .Where(b => b.Id == request.BookingId && b.UserId == request.UserId)
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

        if (b is null)
            return null;

        return new BookingResponse(
            b.Id, b.VenueId, b.Venue.Name, b.Venue.Slug,
            b.StationId, b.Station.Label, b.Station.Kind.ToString(),
            b.StartsAt, b.EndsAt, b.GuestCount,
            b.TotalPrice, b.Currency, b.Status.ToString(),
            b.QrCode.Value, b.CreatedAt);
    }
}
