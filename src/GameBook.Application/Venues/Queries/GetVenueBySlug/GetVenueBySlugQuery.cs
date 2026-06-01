using GameBook.Contracts.Venues;
using MediatR;

namespace GameBook.Application.Venues.Queries.GetVenueBySlug;

public sealed record GetVenueBySlugQuery(string Slug) : IRequest<VenueResponse?>;
