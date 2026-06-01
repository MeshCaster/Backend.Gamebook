using GameBook.Application.Common.Interfaces;
using GameBook.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GameBook.Application.Bookings.Commands.CancelBooking;

public sealed class CancelBookingHandler : IRequestHandler<CancelBookingCommand, bool>
{
    private readonly IGameBookDbContext _db;

    public CancelBookingHandler(IGameBookDbContext db) => _db = db;

    public async Task<bool> Handle(CancelBookingCommand request, CancellationToken ct)
    {
        var booking = await _db.Bookings
            .FirstOrDefaultAsync(b => b.Id == request.BookingId && b.UserId == request.UserId, ct);

        if (booking is null) return false;
        if (booking.Status is BookingStatus.Cancelled or BookingStatus.Completed) return false;

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationReason = request.Reason;
        booking.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);
        return true;
    }
}
