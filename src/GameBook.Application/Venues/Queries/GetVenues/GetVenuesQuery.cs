using GameBook.Contracts.Venues;
using MediatR;

namespace GameBook.Application.Venues.Queries.GetVenues;

public sealed record GetVenuesQuery(string? Search, double? Latitude, double? Longitude) : IRequest<VenueListResponse>;
