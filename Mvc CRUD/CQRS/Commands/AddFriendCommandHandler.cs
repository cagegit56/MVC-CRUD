using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;
using System.Net.NetworkInformation;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class AddFriendCommandHandler : IRequestHandler<AddFriendCommand, Result<string>>
{
    private readonly DataDbContext _context;
    private readonly IUserInfoContextService _currentUser;
    private readonly ILogger<AddFriendCommandHandler> _logger;

    public AddFriendCommandHandler(DataDbContext context, IUserInfoContextService currentUser, ILogger<AddFriendCommandHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(_currentUser));
        _logger = logger;
    }

    public async Task<Result<string>> Handle(AddFriendCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_currentUser.UserId) && string.IsNullOrEmpty(_currentUser.UserName))
            return Result.Fail("Current user userid or username cannot be null.");
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
                    UserId = _currentUser.UserId,
                    FriendId = command.model.FriendId,
                    FriendName = command.model.FriendName,
                    UserName = _currentUser.UserName
                }, new Friends
                {
                    UserId = command.model.FriendId,
                    FriendId = _currentUser.UserId!,
                    FriendName = _currentUser.UserName!,
                    UserName = command.model.FriendName
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

