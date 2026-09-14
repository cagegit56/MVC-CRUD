using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class RemoveProfilePictureCommandHandler : IRequestHandler<RemoveProfilePictureCommand, Result>
{
    private readonly DataDbContext _context;
    private readonly IUserInfoContextService _currentUser;
    private readonly ILogger<RemoveProfilePictureCommandHandler> _logger;

    public RemoveProfilePictureCommandHandler(DataDbContext context, IUserInfoContextService currentUser, ILogger<RemoveProfilePictureCommandHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(_currentUser));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> Handle(RemoveProfilePictureCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _context.Database.BeginTransactionAsync();
        try
        {            
            var res = await _context.Profile.Where(x => x.UserId == _currentUser.UserId)
                .ExecuteUpdateAsync(u => u.SetProperty(p => p.UserProfilePicUrl, ""));

            if (res < 0)
            {
                _logger.LogError("Failed to update profile pic due to no matching record found.");
                return Result.Fail("Failed to update profile pic due to no matching record found.");
            }

            var response = await _context.Post.Where(x => x.UserId == _currentUser.UserId)
                .ExecuteUpdateAsync(u => u.SetProperty(p => p.UserImageUrl, ""));

            await transaction.CommitAsync();

            return Result.Ok();
        }
        catch (Exception ex) {
            await transaction.RollbackAsync();
            _logger.LogError($"Failed to update profile pic due to {ex.Message}");
            return Result.Fail("Failed to update check inner exception for more.");
        }
    }
}

