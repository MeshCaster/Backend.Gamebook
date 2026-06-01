using FluentAssertions;
using GameBook.Application.Services;

namespace GameBook.Application.UnitTests;

public class PricingServiceTests
{
    private readonly PricingService _sut = new();

    [Fact]
    public void CalculateTotal_OneHourOneGuest_ReturnsHourlyRate()
    {
        var start = DateTimeOffset.UtcNow;
        var end = start.AddHours(1);

        var result = _sut.CalculateTotal(10m, start, end, 1);

        result.Should().Be(10m);
    }

    [Fact]
    public void CalculateTotal_TwoHoursTwoGuests_MultipliesCorrectly()
    {
        var start = DateTimeOffset.UtcNow;
        var end = start.AddHours(2);

        var result = _sut.CalculateTotal(8m, start, end, 2);

        result.Should().Be(32m);
    }

    [Fact]
    public void CalculateTotal_HalfHour_CalculatesFraction()
    {
        var start = DateTimeOffset.UtcNow;
        var end = start.AddMinutes(30);

        var result = _sut.CalculateTotal(10m, start, end, 1);

        result.Should().Be(5m);
    }

    [Fact]
    public void CalculateTotal_EndBeforeStart_ThrowsArgumentException()
    {
        var start = DateTimeOffset.UtcNow;
        var end = start.AddHours(-1);

        var act = () => _sut.CalculateTotal(10m, start, end, 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CanCancelForFree_MoreThanTwoHoursAhead_ReturnsTrue()
    {
        var bookingStart = DateTimeOffset.UtcNow.AddHours(3);

        _sut.CanCancelForFree(bookingStart, DateTimeOffset.UtcNow).Should().BeTrue();
    }

    [Fact]
    public void CanCancelForFree_LessThanTwoHoursAhead_ReturnsFalse()
    {
        var bookingStart = DateTimeOffset.UtcNow.AddHours(1);

        _sut.CanCancelForFree(bookingStart, DateTimeOffset.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void CalculateCancellationFee_FreeCancellation_ReturnsZero()
    {
        var bookingStart = DateTimeOffset.UtcNow.AddHours(3);

        var fee = _sut.CalculateCancellationFee(100m, bookingStart, DateTimeOffset.UtcNow);

        fee.Should().Be(0m);
    }

    [Fact]
    public void CalculateCancellationFee_LateCancellation_ReturnsHalfPrice()
    {
        var bookingStart = DateTimeOffset.UtcNow.AddHours(1);

        var fee = _sut.CalculateCancellationFee(100m, bookingStart, DateTimeOffset.UtcNow);

        fee.Should().Be(50m);
    }

    [Fact]
    public void CalculateCancellationFee_AfterStart_ReturnsFullPrice()
    {
        var bookingStart = DateTimeOffset.UtcNow.AddHours(-1);

        var fee = _sut.CalculateCancellationFee(100m, bookingStart, DateTimeOffset.UtcNow);

        fee.Should().Be(100m);
    }
}
