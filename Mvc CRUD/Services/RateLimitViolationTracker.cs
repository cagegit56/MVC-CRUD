using Microsoft.Extensions.Caching.Distributed;
using Mvc_CRUD.Models;
using System.Text.Json;

namespace Mvc_CRUD.Services;

    internal sealed class RateLimitViolationTracker : IRateLimitViolationTracker
    {
        private readonly IDistributedCache _cache;

        public RateLimitViolationTracker(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<(bool blocked, int retryAfter)> IsBlockedAsync(string key)
        {
            var data = await _cache.GetStringAsync(key);
            if (string.IsNullOrEmpty(data))
                return (false, 0);
            var violation = JsonSerializer.Deserialize<RateLimitViolationInfo>(data);
            if (violation == null)
                return (false, 0);
            var remaining = (int)(violation.BlockedUntilUtc - DateTime.UtcNow).TotalSeconds;
            if (remaining > 0)
                return (true, remaining);

            await ClearAsync(key);
            return (false, 0);
        }

        public async Task<int> ExtendBlockAsync(string key)
        {
            RateLimitViolationInfo violation;
            var data = await _cache.GetStringAsync(key); 
            if (string.IsNullOrEmpty(data))
            {
                violation = new RateLimitViolationInfo
                {
                    ViolationSeconds = 15
                };
            }
            else
            {
                violation = JsonSerializer.Deserialize<RateLimitViolationInfo>(data)!;
                violation.ViolationSeconds += 15;
            }

            violation.BlockedUntilUtc = DateTime.UtcNow.AddSeconds(violation.ViolationSeconds);

            await _cache.SetStringAsync( key, JsonSerializer.Serialize(violation),new DistributedCacheEntryOptions {
                    AbsoluteExpiration = violation.BlockedUntilUtc
            });

            return violation.ViolationSeconds;
        }

        public async Task ClearAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }
    }

