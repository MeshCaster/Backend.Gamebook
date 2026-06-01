namespace GameBook.Application.Common.Interfaces;

public interface IPushNotificationService
{
    Task SendAsync(string token, string title, string body, Dictionary<string, object>? data = null, CancellationToken ct = default);
    Task SendToUserAsync(Guid userId, string title, string body, Dictionary<string, object>? data = null, CancellationToken ct = default);
}
