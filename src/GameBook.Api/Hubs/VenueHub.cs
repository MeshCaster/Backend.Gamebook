using Microsoft.AspNetCore.SignalR;

namespace GameBook.Api.Hubs;

public class VenueHub : Hub
{
    public async Task JoinVenueGroup(string venueSlug)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"venue:{venueSlug}");
    }

    public async Task LeaveVenueGroup(string venueSlug)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"venue:{venueSlug}");
    }
}
