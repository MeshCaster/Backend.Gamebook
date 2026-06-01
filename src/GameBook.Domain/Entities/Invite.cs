using GameBook.Domain.Enums;

namespace GameBook.Domain.Entities;

public class Invite
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid InviterId { get; set; }
    public Guid InviteeId { get; set; }
    public InviteStatus Status { get; set; } = InviteStatus.Pending;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public Booking Booking { get; set; } = null!;
    public User Inviter { get; set; } = null!;
    public User Invitee { get; set; } = null!;
}
