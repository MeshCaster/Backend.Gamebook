using GameBook.Application.Common.Interfaces;
using Stripe;

namespace GameBook.Infrastructure.Services;

public class StripePaymentService : IPaymentService
{
    private readonly PaymentIntentService _paymentIntentService;

    public StripePaymentService(string apiKey)
    {
        StripeConfiguration.ApiKey = apiKey;
        _paymentIntentService = new PaymentIntentService();
    }

    public async Task<(string clientSecret, string paymentIntentId)> CreatePaymentIntentAsync(
        decimal amount, string currency, Dictionary<string, string> metadata, CancellationToken ct = default)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100),
            Currency = currency.ToLower(),
            Metadata = metadata,
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true }
        };

        var intent = await _paymentIntentService.CreateAsync(options, cancellationToken: ct);
        return (intent.ClientSecret, intent.Id);
    }

    public async Task<bool> ConfirmPaymentAsync(string paymentIntentId, CancellationToken ct = default)
    {
        var intent = await _paymentIntentService.GetAsync(paymentIntentId, cancellationToken: ct);
        return intent.Status == "succeeded";
    }

    public async Task RefundPaymentAsync(string paymentIntentId, CancellationToken ct = default)
    {
        var refundService = new RefundService();
        await refundService.CreateAsync(new RefundCreateOptions { PaymentIntent = paymentIntentId }, cancellationToken: ct);
    }
}
