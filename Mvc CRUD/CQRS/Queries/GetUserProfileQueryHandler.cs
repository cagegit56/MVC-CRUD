using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Queries;

internal sealed class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfile?>
{
    private readonly DataDbContext _context;
    private readonly IUserInfoContextService _currentUser;
    private readonly IMemoryCache _cache;

    public GetUserProfileQueryHandler(DataDbContext context, IUserInfoContextService currentUser, IMemoryCache cache)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _cache = cache;
    }

    public async Task<UserProfile?> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        try
        {
            string cacheInfo = $"UserInfo-{_currentUser.UserId}";
            if(!_cache.TryGetValue(cacheInfo, out UserProfile? res))
            {
                res = await _context.Profile.Where(x => x.UserId == _currentUser.UserId).FirstOrDefaultAsync();
                _cache.Set(cacheInfo, res, TimeSpan.FromMinutes(10));
            }
            
            return res;
        }
        catch (Exception ex) 
        {
            throw new Exception($"Failed to return data from user profile due to : {ex.Message}");
        }

    }
}

