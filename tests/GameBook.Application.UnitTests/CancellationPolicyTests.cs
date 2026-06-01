using FluentAssertions;
using GameBook.Application.Services;

namespace GameBook.Application.UnitTests;

public class CancellationPolicyTests
{
    private readonly PricingService _sut = new();

    [Theory]
    [InlineData(3, true)]
    [InlineData(2, true)]
    [InlineData(1.99, false)]
    [InlineData(1, false)]
    [InlineData(0.5, false)]
    public void FreeCancellationPolicy_RespectstwohourWindow(double hoursAhead, bool expectedFree)
    {
        var now = DateTimeOffset.UtcNow;
        var bookingStart = now.AddHours(hoursAhead);

        _sut.CanCancelForFree(bookingStart, now).Should().Be(expectedFree);
    }

    [Theory]
    [InlineData(100, 3, 0)]
    [InlineData(100, 1, 50)]
    [InlineData(50, 1, 25)]
    [InlineData(200, 0.5, 100)]
    public void CancellationFee_CalculatesCorrectly(decimal totalPrice, double hoursAhead, decimal expectedFee)
    {
        var now = DateTimeOffset.UtcNow;
        var bookingStart = now.AddHours(hoursAhead);

        var fee = _sut.CalculateCancellationFee(totalPrice, bookingStart, now);

        fee.Should().Be(expectedFee);
    }
}
