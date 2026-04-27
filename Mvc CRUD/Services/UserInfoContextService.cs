using System.Security.Claims;

namespace Mvc_CRUD.Services;

internal sealed class UserInfoContextService : IUserInfoContextService
{
    private readonly IHttpContextAccessor _httpContext;
    public UserInfoContextService(IHttpContextAccessor httpContext)
    {
        _httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
    }

    private ClaimsPrincipal? User => _httpContext.HttpContext?.User;
    public string? UserName =>
        User?.FindFirst(ClaimTypes.Name)?.Value ?? User?.FindFirst("preferred_username")?.Value;

    public string? UserId => User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User?.FindFirst("sub")?.Value;

    public string? FirstName => User?.FindFirst(ClaimTypes.GivenName)?.Value ?? User?.FindFirst("given_name")?.Value;

    public string? LastName => User?.FindFirst(ClaimTypes.Surname)?.Value ?? User?.FindFirst("family_name")?.Value;
   
    public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value;
}

