namespace GameBook.Application.Services;

public class PricingService
{
    public decimal CalculateTotal(decimal pricePerHour, DateTimeOffset startsAt, DateTimeOffset endsAt, int guestCount)
    {
        var hours = (decimal)(endsAt - startsAt).TotalHours;
        if (hours <= 0)
            throw new ArgumentException("End time must be after start time.");
        return Math.Round(pricePerHour * hours * guestCount, 2);
    }

    public bool CanCancelForFree(DateTimeOffset bookingStartsAt, DateTimeOffset now)
    {
        return (bookingStartsAt - now).TotalHours >= 2;
    }

    public decimal CalculateCancellationFee(decimal totalPrice, DateTimeOffset bookingStartsAt, DateTimeOffset now)
    {
        if (CanCancelForFree(bookingStartsAt, now))
            return 0m;

        var hoursUntilStart = (bookingStartsAt - now).TotalHours;
        if (hoursUntilStart < 0)
            return totalPrice;

        return Math.Round(totalPrice * 0.5m, 2);
    }
}
