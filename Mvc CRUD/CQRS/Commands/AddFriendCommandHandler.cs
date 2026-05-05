using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;
using System.Security.Claims;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class AddFriendCommandHandler : IRequestHandler<AddFriendCommand, bool>
{
    private readonly DataDbContext _context;
    private readonly IUserInfoContextService _currentUser;

    public AddFriendCommandHandler(DataDbContext context, IUserInfoContextService currentUser)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(_currentUser));
    }

    public async Task<bool> Handle(AddFriendCommand command, CancellationToken cancellationToken)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            //bool exists = await _context.Friends.AnyAsync(x =>
            //    (x.UserId == _currentUser.UserId && x.FriendId == command.model.FriendId) ||
            //    (x.UserId == command.model.FriendId && x.FriendId == _currentUser.UserId)
            //);
            //if (exists) return true;

            var frnd = new Friends()
            {
                UserId = command.model.FriendId,
                FriendId = _currentUser.UserId!,
                Status = "online",
                FriendName = _currentUser.UserName!,
                UserName = command.model.FriendName              
            };
            var frnd2 = new Friends()
            {
                UserId = _currentUser.UserId!,
                FriendId = command.model.FriendId,
                Status = "online",
                FriendName = command.model.FriendName,
                UserName = _currentUser.UserName!
            };
            await _context.Friends.AddRangeAsync(frnd, frnd2);
            await _context.SaveChangesAsync(cancellationToken);

            var accepted = await _context.FriendRequests.Where(x => (x.UserId == command.model.FriendId && x.ToUserId == _currentUser.UserId)
                                 || (x.UserId == _currentUser.UserId && x.ToUserId == command.model.FriendId) && x.Status == "Pending").ToListAsync();
            if (accepted.Any())
            {
                foreach (var data in accepted)
                {
                    data.Status = "Accepted";
                    _context.FriendRequests.Update(data);
                    await _context.SaveChangesAsync(cancellationToken);
                }               
            }

            await transaction.CommitAsync(cancellationToken);
            return true;

        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new Exception($"Failed to accept friend request due to: {ex.Message}");
        }
    }
}

