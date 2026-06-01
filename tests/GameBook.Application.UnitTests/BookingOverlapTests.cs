using FluentAssertions;
using GameBook.Domain.Constants;
using GameBook.Domain.Entities;
using GameBook.Domain.Enums;

namespace GameBook.Application.UnitTests;

public class BookingOverlapTests
{
    private static Booking CreateBooking(DateTimeOffset start, DateTimeOffset end, Guid stationId) => new()
    {
        Id = Guid.NewGuid(),
        StationId = stationId,
        StartsAt = start,
        EndsAt = end,
        Status = BookingStatus.Confirmed
    };

    private static bool HasOverlapWithBuffer(Booking existing, DateTimeOffset newStart, DateTimeOffset newEnd)
    {
        return existing.Status != BookingStatus.Cancelled
            && existing.StartsAt < newEnd
            && existing.EndsAt.AddMinutes(BookingRules.BufferMinutes) > newStart;
    }

    [Fact]
    public void Bookings_SameStationOverlapping_AreDetected()
    {
        var stationId = Guid.NewGuid();
        var existing = CreateBooking(
            DateTimeOffset.UtcNow.AddHours(1),
            DateTimeOffset.UtcNow.AddHours(3),
            stationId);

        var newStart = DateTimeOffset.UtcNow.AddHours(2);
        var newEnd = DateTimeOffset.UtcNow.AddHours(4);

        HasOverlapWithBuffer(existing, newStart, newEnd).Should().BeTrue();
    }

    [Fact]
    public void Bookings_AdjacentSameStation_OverlapDueToBuffer()
    {
        var stationId = Guid.NewGuid();
        var existing = CreateBooking(
            DateTimeOffset.UtcNow.AddHours(1),
            DateTimeOffset.UtcNow.AddHours(2),
            stationId);

        // New booking starts exactly when existing ends — falls within buffer
        var newStart = DateTimeOffset.UtcNow.AddHours(2);
        var newEnd = DateTimeOffset.UtcNow.AddHours(3);

        HasOverlapWithBuffer(existing, newStart, newEnd).Should().BeTrue();
    }

    [Fact]
    public void Bookings_StartingWithinBuffer_AreDetected()
    {
        var stationId = Guid.NewGuid();
        var existing = CreateBooking(
            DateTimeOffset.UtcNow.AddHours(1),
            DateTimeOffset.UtcNow.AddHours(2),
            stationId);

        // New booking starts 5 minutes after existing ends — within 10-min buffer
        var newStart = existing.EndsAt.AddMinutes(5);
        var newEnd = newStart.AddHours(1);

        HasOverlapWithBuffer(existing, newStart, newEnd).Should().BeTrue();
    }

    [Fact]
    public void Bookings_StartingAfterBuffer_DoNotOverlap()
    {
        var stationId = Guid.NewGuid();
        var existing = CreateBooking(
            DateTimeOffset.UtcNow.AddHours(1),
            DateTimeOffset.UtcNow.AddHours(2),
            stationId);

        // New booking starts 10+ minutes after existing ends — clears buffer
        var newStart = existing.EndsAt.AddMinutes(BookingRules.BufferMinutes);
        var newEnd = newStart.AddHours(1);

        HasOverlapWithBuffer(existing, newStart, newEnd).Should().BeFalse();
    }

    [Fact]
    public void Bookings_DifferentStations_NeverOverlap()
    {
        var existing = CreateBooking(
            DateTimeOffset.UtcNow.AddHours(1),
            DateTimeOffset.UtcNow.AddHours(3),
            Guid.NewGuid());

        var newStationId = Guid.NewGuid();
        var newStart = DateTimeOffset.UtcNow.AddHours(2);
        var newEnd = DateTimeOffset.UtcNow.AddHours(4);

        var sameStation = existing.StationId == newStationId;
        var overlaps = sameStation && HasOverlapWithBuffer(existing, newStart, newEnd);

        overlaps.Should().BeFalse();
    }

    [Fact]
    public void CancelledBooking_DoesNotConflict()
    {
        var stationId = Guid.NewGuid();
        var existing = CreateBooking(
            DateTimeOffset.UtcNow.AddHours(1),
            DateTimeOffset.UtcNow.AddHours(3),
            stationId);
        existing.Status = BookingStatus.Cancelled;

        var newStart = DateTimeOffset.UtcNow.AddHours(2);
        var newEnd = DateTimeOffset.UtcNow.AddHours(4);

        HasOverlapWithBuffer(existing, newStart, newEnd).Should().BeFalse();
    }
}
