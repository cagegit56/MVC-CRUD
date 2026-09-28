using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Mvc_CRUD.Dto;
using Mvc_CRUD.Models;
using System.Security.Claims;

namespace Mvc_CRUD.Services;

public class CurrentUserProfileService : ICurrentUserProfile
{
    private readonly DataDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly IHttpContextAccessor _httpContext;
    private readonly ILogger<CurrentUserProfileService> _logger;

    public CurrentUserProfileService(DataDbContext context, IHttpContextAccessor httpContext, IMemoryCache cache,
        ILogger<CurrentUserProfileService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger;
    }

    private ClaimsPrincipal? User => _httpContext.HttpContext?.User;
    public UserProfileDTO? _User;
    public string? currentUserId => User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User?.FindFirst("sub")?.Value;

    public UserProfileDTO? UserInfo
    {
        get 
        {
            try
            {
                if (_User != null) return _User;
                if (string.IsNullOrEmpty(currentUserId)) return null;
                var CacheKey = $"UserProfile-{currentUserId}";
                if (!_cache.TryGetValue(CacheKey, out UserProfileDTO? res))
                {
                    res = _context.Profile.AsNoTracking().Where(x => x.UserId == currentUserId)
                        .Select(m => new UserProfileDTO()
                        {
                            Id = m.Id,
                            UserName = m.UserName,
                            LastName = m.LastName,
                            UserId = m.UserId,
                            UserProfilePicUrl = m.UserProfilePicUrl,
                            UserCoverPicUrl = m.UserCoverPicUrl,
                            Email = m.Email,
                            Bio = m.Bio,
                            Location = m.Location,
                            HighSchoolName = m.HighSchoolName,
                            Subject = m.Subject,
                            CollegeName = m.CollegeName,
                            Course = m.Course,
                            RelationShipStatus = m.RelationShipStatus,
                            Website = m.Website,
                            FromLocation = m.FromLocation,
                            SchoolPeriod = m.SchoolPeriod,
                            CollegePeriod = m.CollegePeriod,
                            JobTitle = m.JobTitle,
                            Industry = m.Industry,
                            JobPeriod = m.JobPeriod,
                        }).FirstOrDefault();
                    _cache.Set(CacheKey, res, TimeSpan.FromMinutes(10));
                }
                _User = res;
                return _User;
            }
            catch (Exception ex) {
                _logger.LogError($"Failed to return user profile due to : {ex.Message}");
                _User!.Error = "Failed to return user profile due to technical issue";
                return _User;
            }        
        }             
    }

    public string? UserId => UserInfo?.UserId;
    public string? UserName => UserInfo?.UserName;
    public string? Email => UserInfo?.Email;
    public string? LastName => UserInfo?.LastName;
    public string? ProfilePicUrl => UserInfo?.UserProfilePicUrl;

    public UserProfileDTO? UserProfileObj => UserInfo;
}

