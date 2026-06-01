using GameBook.Application.Common.Interfaces;
using GameBook.Application.Services;
using GameBook.Contracts.Bookings;
using GameBook.Domain.Constants;
using GameBook.Domain.Entities;
using GameBook.Domain.Enums;
using GameBook.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GameBook.Application.Bookings.Commands.CreateBooking;

public sealed class CreateBookingHandler : IRequestHandler<CreateBookingCommand, BookingResponse>
{
    private readonly IGameBookDbContext _db;
    private readonly PricingService _pricing;

    public CreateBookingHandler(IGameBookDbContext db, PricingService pricing)
    {
        _db = db;
        _pricing = pricing;
    }

    public async Task<BookingResponse> Handle(CreateBookingCommand request, CancellationToken ct)
    {
        var station = await _db.Stations
            .Include(s => s.Venue)
            .FirstOrDefaultAsync(s => s.Id == request.StationId && s.IsActive, ct)
            ?? throw new InvalidOperationException("Station not found or inactive.");

        var hasOverlap = await _db.Bookings.AnyAsync(b =>
            b.StationId == request.StationId
            && b.Status != BookingStatus.Cancelled
            && b.StartsAt < request.EndsAt
            && b.EndsAt.AddMinutes(BookingRules.BufferMinutes) > request.StartsAt, ct);

        if (hasOverlap)
            throw new InvalidOperationException(
                $"This time slot overlaps with an existing booking or its {BookingRules.BufferMinutes}-minute cleanup buffer.");

        var totalPrice = _pricing.CalculateTotal(station.PricePerHour, request.StartsAt, request.EndsAt, request.GuestCount);

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            VenueId = request.VenueId,
            StationId = request.StationId,
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
            GuestCount = request.GuestCount,
            TotalPrice = totalPrice,
            Status = BookingStatus.Pending,
            QrCode = QrRef.Generate(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync(ct);

        return new BookingResponse(
            booking.Id, booking.VenueId, station.Venue.Name, station.Venue.Slug,
            booking.StationId, station.Label, station.Kind.ToString(),
            booking.StartsAt, booking.EndsAt, booking.GuestCount,
            booking.TotalPrice, booking.Currency, booking.Status.ToString(),
            booking.QrCode.Value, booking.CreatedAt);
    }
}
