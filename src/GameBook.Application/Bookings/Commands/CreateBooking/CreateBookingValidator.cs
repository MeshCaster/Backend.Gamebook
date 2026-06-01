using FluentValidation;
using GameBook.Domain.Constants;

namespace GameBook.Application.Bookings.Commands.CreateBooking;

public class CreateBookingValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingValidator()
    {
        RuleFor(x => x.VenueId).NotEmpty();
        RuleFor(x => x.StationId).NotEmpty();
        RuleFor(x => x.StartsAt).GreaterThan(DateTimeOffset.UtcNow.AddMinutes(-5));
        RuleFor(x => x.EndsAt).GreaterThan(x => x.StartsAt).WithMessage("End time must be after start time.");
        RuleFor(x => x.GuestCount).InclusiveBetween(1, 10);

        RuleFor(x => x)
            .Must(x => (x.EndsAt - x.StartsAt).TotalMinutes >= BookingRules.MinimumBookingMinutes)
            .WithMessage($"Booking must be at least {BookingRules.MinimumBookingMinutes} minutes.");
    }
}
