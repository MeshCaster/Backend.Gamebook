namespace GameBook.Contracts.Payments;

public sealed record CreatePaymentIntentRequest(
    Guid BookingId);

public sealed record PaymentIntentResponse(
    string ClientSecret,
    string PaymentIntentId,
    decimal Amount,
    string Currency);
