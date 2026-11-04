using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
namespace Mvc_CRUD.Services;

public class SignalRHub : Hub
{
    private static readonly ConcurrentDictionary<string, string> _connections = new();
    private readonly ICurrentUserProfile _currentUser;
    private readonly SignalRConnectedUserId _connectedUserIdChats;

    public SignalRHub(ICurrentUserProfile currentUser, SignalRConnectedUserId connectedUserIdChats)
    {
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _connectedUserIdChats = connectedUserIdChats ?? throw new ArgumentNullException(nameof(_connectedUserIdChats));
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

    public void LiveChat() => _connectedUserIdChats.AddUserId(_currentUser.UserId!);    

    public async Task IsTypingMessage(string toUser, string userId)
    {
        if (_connectedUserIdChats.CheckUserId(userId))
        {
            if (_connections.TryGetValue(toUser, out var connectionId))
                await Clients.Client(connectionId).SendAsync("TypingMessage");
        }
    } 

    public async Task SendPrivateMessage(string toUser, string message, string userId)
    {
        if (_connections.TryGetValue(toUser, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("ReceiveMessage", _currentUser.UserName, message, _currentUser.UserId, _currentUser.ProfilePicUrl, _currentUser.LastName);
            await Clients.Caller.SendAsync("MessageConfirmation", userId);
        }
    }
    public async Task SendGroupMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveGroupMessage", user, message);
    }

}

