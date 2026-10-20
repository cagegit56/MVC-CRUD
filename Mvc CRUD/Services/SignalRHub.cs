using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
namespace Mvc_CRUD.Services;

public class SignalRHub : Hub
{
    private static readonly ConcurrentDictionary<string, string> _connections = new();
    private readonly ICurrentUserProfile _currentUser;

    public SignalRHub(ICurrentUserProfile currentUser)
    {
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public override async Task OnConnectedAsync()
    {
        var username = Context.User?.FindFirst("preferred_username")?.Value ?? Context.User?.Identity?.Name;
        if (!string.IsNullOrEmpty(username))
        {
            _connections[username] = Context.ConnectionId;
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var username = Context.User?.FindFirst("preferred_username")?.Value ?? Context.User?.Identity?.Name;
        if (!string.IsNullOrEmpty(username))
        {
            _connections.TryRemove(username, out _);
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendPrivateMessage(string toUser, string message, string userId)
    {
        var fromUser = Context.User?.FindFirst("preferred_username")?.Value ?? Context.User?.Identity?.Name;

        if (_connections.TryGetValue(toUser, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("ReceiveMessage", fromUser, message, _currentUser.UserId, _currentUser.ProfilePicUrl);
            await Clients.Caller.SendAsync("MessageConfirmation", userId);
        }
    }
    public async Task SendGroupMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveGroupMessage", user, message);
    }

}

