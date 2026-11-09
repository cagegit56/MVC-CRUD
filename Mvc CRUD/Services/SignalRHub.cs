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
        _connections[_currentUser.UserName!] = Context.ConnectionId;
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _connections.TryRemove(_currentUser.UserName!, out _);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task IsTypingMessage(string toUser)
    {
        if (_connections.TryGetValue(toUser, out var connectionId))
            await Clients.Client(connectionId).SendAsync("TypingMessage", _currentUser.UserId);            
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

