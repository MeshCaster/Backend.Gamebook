using GameBook.Application.Common.Interfaces;
using GameBook.Domain.Entities;
using Meshcaster.IdentityProvider.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GameBook.Infrastructure.Persistence;

public class GameBookDbContext : DbContext, IGameBookDbContext, IIdentityUserDbContext<User>
{
    public GameBookDbContext(DbContextOptions<GameBookDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Station> Stations => Set<Station>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Invite> Invites => Set<Invite>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<PushToken> PushTokens => Set<PushToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("btree_gist");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GameBookDbContext).Assembly);
    }
}
