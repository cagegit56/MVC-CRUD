using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class AddFriendCommandHandler : IRequestHandler<AddFriendCommand, Result<string>>
{
    private readonly DataDbContext _context;
    private readonly ICurrentUserProfile _currentUser;
    private readonly ILogger<AddFriendCommandHandler> _logger;

    public AddFriendCommandHandler(DataDbContext context, ICurrentUserProfile currentUser, ILogger<AddFriendCommandHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(_currentUser));
        _logger = logger;
    }

    public async Task<Result<string>> Handle(AddFriendCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(command.model.FriendId) || string.IsNullOrEmpty(command.model.FriendName) || string.IsNullOrEmpty(command.model.FriendLastName))
            return Result.Fail("Friend Id,Name, or Lastname cannot be null.");

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            bool exists = await _context.Friends.AnyAsync(x =>
                (x.UserId == _currentUser.UserId && x.FriendId == command.model.FriendId) ||
                (x.UserId == command.model.FriendId && x.FriendId == _currentUser.UserId)
            );
            if (exists) return Result.Ok();

            await _context.Friends.AddRangeAsync(new Friends
                {
                    UserId = _currentUser.UserId!,                   
                    UserName = _currentUser.UserName!,
                    LastName = _currentUser.LastName!,
                    ProfilePicUrl = _currentUser.ProfilePicUrl,
                    FriendId = command.model.FriendId,
                    FriendName = command.model.FriendName,
                    FriendLastName = command.model.FriendLastName,
                    FriendProfilePicUrl = command.model.FriendProfilePicUrl,
                }, new Friends
                {
                    UserId = command.model.FriendId,
                    UserName = command.model.FriendName,
                    LastName = command.model.FriendLastName,
                    ProfilePicUrl = command.model.FriendProfilePicUrl,
                    FriendId = _currentUser.UserId!,
                    FriendName = _currentUser.UserName!,
                    FriendLastName = _currentUser.LastName!,
                    FriendProfilePicUrl = _currentUser.ProfilePicUrl,                     
                }
            );

            var res = await _context.FriendRequests.Where(x => 
                                    ( (x.UserId == command.model.FriendId && x.ToUserId == _currentUser.UserId)
                                    || (x.UserId == _currentUser.UserId && x.ToUserId == command.model.FriendId) ) 
                                    && x.Status == "Pending")
                    .ExecuteUpdateAsync(g => g.SetProperty(y => y.Status, "Accepted"), cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError($"Failed to accept friend request due to: {ex.Message}");
            return Result.Fail("Failed to accept friend request, Please see inner exception for more info.");
        }
    }  
}

