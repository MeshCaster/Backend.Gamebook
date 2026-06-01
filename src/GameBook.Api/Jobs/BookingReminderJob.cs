using GameBook.Application.Common.Interfaces;
using GameBook.Infrastructure.Persistence;
using GameBook.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GameBook.Api.Jobs;

public class BookingReminderJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingReminderJob> _logger;

    public BookingReminderJob(IServiceScopeFactory scopeFactory, ILogger<BookingReminderJob> logger)
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
                var pushService = scope.ServiceProvider.GetRequiredService<IPushNotificationService>();

                var reminderWindow = DateTimeOffset.UtcNow.AddMinutes(30);
                var upcoming = await db.Bookings
                    .Include(b => b.Venue)
                    .Include(b => b.Station)
                    .Where(b => b.Status == BookingStatus.Confirmed
                        && b.StartsAt <= reminderWindow
                        && b.StartsAt > DateTimeOffset.UtcNow)
                    .ToListAsync(stoppingToken);

                foreach (var booking in upcoming)
                {
                    await pushService.SendToUserAsync(
                        booking.UserId,
                        "Booking Reminder",
                        $"Your session at {booking.Venue.Name} starts in 30 minutes!",
                        new Dictionary<string, object> { ["bookingId"] = booking.Id.ToString() },
                        stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BookingReminderJob");
            }

            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }
    }
}
