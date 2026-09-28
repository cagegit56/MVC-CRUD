using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class RemoveCoverPicureCommandHandler : IRequestHandler<RemoveCoverPicureCommand, Result>
{
    private readonly DataDbContext _context;
    private readonly IUserInfoContextService _currentUser;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RemoveProfilePictureCommandHandler> _logger;

    public RemoveCoverPicureCommandHandler(DataDbContext context, IUserInfoContextService currentUser, IMemoryCache cache,
        ILogger<RemoveProfilePictureCommandHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(_currentUser));
        _cache = cache ?? throw new ArgumentNullException(nameof(_cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> Handle(RemoveCoverPicureCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var res = await _context.Profile.Where(x => x.UserId == _currentUser.UserId)
                .ExecuteUpdateAsync(u => u.SetProperty(p => p.UserCoverPicUrl, ""));

            if (res < 0)
            {
                _logger.LogError("Failed to update cover pic due to no matching record found.");
                return Result.Fail("Failed to update cover pic due to no matching record found.");
            }

            _cache.Remove($"UserProfile-{_currentUser.UserId}");
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to update cover pic due to {ex.Message}");
            return Result.Fail("Failed to update check inner exception for more.");
        }
    }
}

