namespace GameBook.Application.Common.Interfaces;

public interface IPaymentService
{
    Task<(string clientSecret, string paymentIntentId)> CreatePaymentIntentAsync(
        decimal amount, string currency, Dictionary<string, string> metadata, CancellationToken ct = default);
    Task<bool> ConfirmPaymentAsync(string paymentIntentId, CancellationToken ct = default);
    Task RefundPaymentAsync(string paymentIntentId, CancellationToken ct = default);
}
