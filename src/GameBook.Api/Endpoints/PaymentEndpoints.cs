using GameBook.Application.Common.Interfaces;
using Meshcaster.IdentityProvider.Abstractions;
using GameBook.Contracts.Payments;
using GameBook.Domain.Entities;
using GameBook.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GameBook.Api.Endpoints;

public static class PaymentEndpoints
{
    public static RouteGroupBuilder MapPaymentEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/v1/payments").WithTags("Payments");

        group.MapPost("/intent", async (CreatePaymentIntentRequest request, ICurrentUser user,
            IPaymentService paymentService, IGameBookDbContext db) =>
        {
            var booking = await db.Bookings.FirstOrDefaultAsync(b =>
                b.Id == request.BookingId && b.UserId == user.UserId);

            if (booking is null) return Results.NotFound();

            var metadata = new Dictionary<string, string>
            {
                ["booking_id"] = booking.Id.ToString(),
                ["user_id"] = user.UserId.ToString()
            };

            var (clientSecret, paymentIntentId) = await paymentService.CreatePaymentIntentAsync(
                booking.TotalPrice, booking.Currency, metadata);

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                UserId = user.UserId,
                Amount = booking.TotalPrice,
                Currency = booking.Currency,
                Provider = PaymentProvider.Stripe,
                Status = PaymentStatus.Pending,
                ProviderPaymentId = paymentIntentId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            db.Payments.Add(payment);
            await db.SaveChangesAsync();

            return Results.Ok(new PaymentIntentResponse(clientSecret, paymentIntentId, booking.TotalPrice, booking.Currency));
        }).RequireAuthorization().WithName("CreatePaymentIntent");

        return group;
    }
}
