using GameBook.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameBook.Application.Common.Interfaces;

public interface IGameBookDbContext
{
    DbSet<User> Users { get; }
    DbSet<Venue> Venues { get; }
    DbSet<Station> Stations { get; }
    DbSet<Booking> Bookings { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Review> Reviews { get; }
    DbSet<Invite> Invites { get; }
    DbSet<Wallet> Wallets { get; }
    DbSet<PushToken> PushTokens { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
