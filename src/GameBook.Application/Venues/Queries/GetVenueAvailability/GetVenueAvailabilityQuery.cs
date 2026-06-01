using GameBook.Contracts.Venues;
using MediatR;

namespace GameBook.Application.Venues.Queries.GetVenueAvailability;

public sealed record GetVenueAvailabilityQuery(string Slug, DateOnly Date) : IRequest<VenueAvailabilityResponse?>;
