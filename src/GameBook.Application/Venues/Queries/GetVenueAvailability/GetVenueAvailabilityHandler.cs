using GameBook.Application.Common.Interfaces;
using GameBook.Contracts.Venues;
using GameBook.Domain.Constants;
using GameBook.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GameBook.Application.Venues.Queries.GetVenueAvailability;

public sealed class GetVenueAvailabilityHandler : IRequestHandler<GetVenueAvailabilityQuery, VenueAvailabilityResponse?>
{
    private readonly IGameBookDbContext _db;

    public GetVenueAvailabilityHandler(IGameBookDbContext db) => _db = db;

    public async Task<VenueAvailabilityResponse?> Handle(GetVenueAvailabilityQuery request, CancellationToken ct)
    {
        var venue = await _db.Venues
            .Include(v => v.Stations.Where(s => s.IsActive))
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Slug == request.Slug && v.IsActive, ct);

        if (venue is null) return null;

        var dayStart = request.Date.ToDateTime(venue.OpensAt, DateTimeKind.Utc);
        var dayEnd = request.Date.ToDateTime(venue.ClosesAt, DateTimeKind.Utc);
        if (dayEnd <= dayStart) dayEnd = dayEnd.AddDays(1);

        var dayStartOffset = new DateTimeOffset(dayStart, TimeSpan.Zero);
        var dayEndOffset = new DateTimeOffset(dayEnd, TimeSpan.Zero);

        var now = DateTimeOffset.UtcNow;
        var effectiveStart = now > dayStartOffset && now < dayEndOffset
            ? now
            : dayStartOffset;

        var existingBookings = await _db.Bookings
            .Where(b => b.VenueId == venue.Id
                && b.StartsAt < dayEndOffset
                && b.EndsAt > dayStartOffset
                && b.Status != BookingStatus.Cancelled)
            .AsNoTracking()
            .ToListAsync(ct);

        var stations = new List<StationAvailability>();

        foreach (var station in venue.Stations)
        {
            var stationBookings = existingBookings
                .Where(b => b.StationId == station.Id)
                .OrderBy(b => b.StartsAt)
                .ToList();

            var blockedIntervals = stationBookings
                .Select(b => (Start: b.StartsAt, End: b.EndsAt.AddMinutes(BookingRules.BufferMinutes)))
                .ToList();

            var windows = new List<AvailableWindow>();
            var cursor = effectiveStart;

            foreach (var blocked in blockedIntervals)
            {
                if (blocked.Start > cursor)
                {
                    var windowEnd = blocked.Start < dayEndOffset ? blocked.Start : dayEndOffset;
                    if ((windowEnd - cursor).TotalMinutes >= BookingRules.MinimumBookingMinutes)
                        windows.Add(new AvailableWindow(cursor, windowEnd));
                }

                if (blocked.End > cursor)
                    cursor = blocked.End;
            }

            if (cursor < dayEndOffset
                && (dayEndOffset - cursor).TotalMinutes >= BookingRules.MinimumBookingMinutes)
            {
                windows.Add(new AvailableWindow(cursor, dayEndOffset));
            }

            stations.Add(new StationAvailability(
                station.Id,
                station.Label,
                station.Kind.ToString(),
                station.PricePerHour,
                windows));
        }

        return new VenueAvailabilityResponse(venue.Id, request.Date, stations);
    }
}
