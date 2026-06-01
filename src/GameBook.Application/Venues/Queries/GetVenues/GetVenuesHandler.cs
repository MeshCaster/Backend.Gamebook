using GameBook.Application.Common.Interfaces;
using GameBook.Contracts.Venues;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace GameBook.Application.Venues.Queries.GetVenues;

public sealed class GetVenuesHandler : IRequestHandler<GetVenuesQuery, VenueListResponse>
{
    private readonly IGameBookDbContext _db;

    public GetVenuesHandler(IGameBookDbContext db) => _db = db;

    public async Task<VenueListResponse> Handle(GetVenuesQuery request, CancellationToken ct)
    {
        var query = _db.Venues
            .Where(v => v.IsActive)
            .Include(v => v.Stations.Where(s => s.IsActive))
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(v => v.Name.ToLower().Contains(search) || v.Address.ToLower().Contains(search));
        }

        Point? userLocation = null;
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            userLocation = new Point(request.Longitude.Value, request.Latitude.Value) { SRID = 4326 };
            query = query.OrderBy(v => v.Location == null ? double.MaxValue : v.Location.Distance(userLocation));
        }
        else
        {
            query = query.OrderByDescending(v => v.Rating);
        }

        var venues = await query.ToListAsync(ct);

        var response = venues.Select(v => new VenueResponse(
            v.Id, v.Name, v.Slug, v.Description, v.Address,
            v.ImageUrl, v.CoverUrl,
            v.Location?.Y, v.Location?.X,
            v.Rating, v.ReviewCount,
            v.OpensAt.ToString("HH:mm"), v.ClosesAt.ToString("HH:mm"),
            v.Phone, v.Website, v.Amenities,
            v.Stations.Select(s => new StationResponse(
                s.Id, s.Label, s.Kind.ToString(), s.PricePerHour, s.Capacity, s.Specs
            )).ToList(),
            IsFeatured: v.IsFeatured,
            DistanceKm: userLocation != null && v.Location != null
                ? HaversineKm(v.Location.Y, v.Location.X, userLocation.Y, userLocation.X)
                : null
        )).ToList();

        return new VenueListResponse(response, response.Count);
    }

    private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double r = 6371.0;
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return Math.Round(r * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)), 2);
    }

    private static double ToRad(double deg) => deg * Math.PI / 180.0;
}
