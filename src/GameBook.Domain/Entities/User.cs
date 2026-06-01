using Meshcaster.IdentityProvider.Entities;

namespace GameBook.Domain.Entities;

public class User : IdentityUserBase
{
    public string? GamerTag { get; set; }

    public Wallet? Wallet { get; set; }
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<PushToken> PushTokens { get; set; } = new List<PushToken>();
}
