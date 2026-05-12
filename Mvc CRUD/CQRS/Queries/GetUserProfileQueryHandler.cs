using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Mvc_CRUD.Dto;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Queries;

internal sealed class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDTO?>
{
    private readonly DataDbContext _context;
    private readonly IUserInfoContextService _currentUser;
    private readonly IMemoryCache _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<GetUserProfileQueryHandler> _logger;

    public GetUserProfileQueryHandler(DataDbContext context, IUserInfoContextService currentUser, IMemoryCache cache, 
                                       ILogger<GetUserProfileQueryHandler> logger, IMapper mapper)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _cache = cache;
        _logger = logger;
    }

    public async Task<UserProfileDTO?> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var res = new UserProfileDTO();
        try
        {
            string cacheInfo = $"UserInfo-{_currentUser.UserId}";            
            if(!_cache.TryGetValue(cacheInfo, out UserProfile? results))
            {
                results = await _context.Profile.Where(x => x.UserId == _currentUser.UserId).AsNoTracking().FirstOrDefaultAsync();
                res = _mapper.Map<UserProfileDTO>(results);
                _cache.Set(cacheInfo, res, TimeSpan.FromMinutes(10));
            }            
            return res;
        }
        catch (Exception ex) 
        {
            _logger.LogError($"Failed to get user's profile info due to : {ex.Message}");
            res.Errors = "Failed to get user's profile info";
            return res;
        }

    }
}

