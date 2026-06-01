using GameBook.Domain.Enums;

namespace GameBook.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "GEL";
    public PaymentProvider Provider { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? ProviderPaymentId { get; set; }
    public string? ProviderSessionId { get; set; }
    public string? ReceiptUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public Booking Booking { get; set; } = null!;
    public User User { get; set; } = null!;
}
