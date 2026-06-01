using GameBook.Application.Common.Interfaces;
using GameBook.Contracts.Venues;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GameBook.Application.Venues.Queries.GetVenueBySlug;

public sealed class GetVenueBySlugHandler : IRequestHandler<GetVenueBySlugQuery, VenueResponse?>
{
    private readonly IGameBookDbContext _db;

    public GetVenueBySlugHandler(IGameBookDbContext db) => _db = db;

    public async Task<VenueResponse?> Handle(GetVenueBySlugQuery request, CancellationToken ct)
    {
        var v = await _db.Venues
            .Include(v => v.Stations.Where(s => s.IsActive))
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Slug == request.Slug && v.IsActive, ct);

        if (v is null) return null;

        return new VenueResponse(
            v.Id, v.Name, v.Slug, v.Description, v.Address,
            v.ImageUrl, v.CoverUrl,
            v.Location?.Y, v.Location?.X,
            v.Rating, v.ReviewCount,
            v.OpensAt.ToString("HH:mm"), v.ClosesAt.ToString("HH:mm"),
            v.Phone, v.Website, v.Amenities,
            v.Stations.Select(s => new StationResponse(
                s.Id, s.Label, s.Kind.ToString(), s.PricePerHour, s.Capacity, s.Specs
            )).ToList(),
            IsFeatured: v.IsFeatured
        );
    }
}
