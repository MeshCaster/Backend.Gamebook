using System.Net.Http.Json;
using GameBook.Application.Common.Interfaces;
using GameBook.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GameBook.Infrastructure.Services;

public class ExpoPushService : IPushNotificationService
{
    private readonly HttpClient _httpClient;
    private readonly IServiceScopeFactory _scopeFactory;

    public ExpoPushService(HttpClient httpClient, IServiceScopeFactory scopeFactory)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://exp.host/--/api/v2/push/");
        _scopeFactory = scopeFactory;
    }

    public async Task SendAsync(string token, string title, string body, Dictionary<string, object>? data = null, CancellationToken ct = default)
    {
        var payload = new { to = token, title, body, data, sound = "default" };
        await _httpClient.PostAsJsonAsync("send", payload, ct);
    }

    public async Task SendToUserAsync(Guid userId, string title, string body, Dictionary<string, object>? data = null, CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GameBookDbContext>();

        var tokens = await db.PushTokens
            .Where(t => t.UserId == userId && t.IsActive)
            .Select(t => t.Token)
            .ToListAsync(ct);

        foreach (var token in tokens)
        {
            await SendAsync(token, title, body, data, ct);
        }
    }
}
