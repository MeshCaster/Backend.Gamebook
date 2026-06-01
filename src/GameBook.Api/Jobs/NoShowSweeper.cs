using GameBook.Infrastructure.Persistence;
using GameBook.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GameBook.Api.Jobs;

public class NoShowSweeper : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NoShowSweeper> _logger;

    public NoShowSweeper(IServiceScopeFactory scopeFactory, ILogger<NoShowSweeper> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<GameBookDbContext>();

                var cutoff = DateTimeOffset.UtcNow.AddMinutes(-15);
                var noShows = await db.Bookings
                    .Where(b => b.Status == BookingStatus.Confirmed && b.StartsAt < cutoff)
                    .ToListAsync(stoppingToken);

                foreach (var booking in noShows)
                {
                    booking.Status = BookingStatus.NoShow;
                    booking.UpdatedAt = DateTimeOffset.UtcNow;
                }

                if (noShows.Count > 0)
                {
                    await db.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Marked {Count} bookings as no-show", noShows.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NoShowSweeper");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
