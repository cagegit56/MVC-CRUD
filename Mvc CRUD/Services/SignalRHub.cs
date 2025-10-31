using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace Mvc_CRUD.Services;

    public class SignalRHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> _connections = new();

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

        public async Task SendPrivateMessage(string toUser, string message)
        {
            var fromUser = Context.User?.FindFirst("preferred_username")?.Value ?? Context.User?.Identity?.Name;
            if (_connections.TryGetValue(toUser, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", fromUser, message);
            }
            await Clients.Caller.SendAsync("MessageConfirmation", toUser, message);
        }
        public async Task SendGroupMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveGroupMessage", user, message);
        }
    }

