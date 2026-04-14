namespace Mvc_CRUD.Services;

    public interface IRateLimitViolationTracker
    {
        Task<(bool blocked, int retryAfter)> IsBlockedAsync(string key);
        Task<int> ExtendBlockAsync(string key);
        Task ClearAsync(string key);
    }

