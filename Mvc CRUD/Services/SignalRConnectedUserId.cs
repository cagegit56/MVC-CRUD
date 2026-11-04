using System.Collections.Concurrent;

namespace Mvc_CRUD.Services;

    public class SignalRConnectedUserId
    {
        private readonly ConcurrentDictionary<string, byte> LiveChat_UserId = new();

        public void AddUserId(string userId)
        {
           LiveChat_UserId.TryAdd(userId, 0);
        }

        public bool CheckUserId(string userId)
        {
            return LiveChat_UserId.ContainsKey(userId);
        }

        public void RemoveUserId(string userId)
        {
            LiveChat_UserId.TryRemove(userId, out _);
        }
    }
