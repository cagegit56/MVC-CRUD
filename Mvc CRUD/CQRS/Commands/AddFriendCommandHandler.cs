using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;
using System.Security.Claims;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class AddFriendCommandHandler : IRequestHandler<AddFriendCommand, string>
{
    private readonly DataDbContext _context;

    public AddFriendCommandHandler(DataDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<string> Handle(AddFriendCommand command, CancellationToken cancellationToken)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            bool exists = await _context.Friends.AnyAsync(x =>
                (x.UserId == command.model.UserId && x.FriendId == command.model.FriendId) ||
                (x.UserId == command.model.FriendId && x.FriendId == command.model.UserId)
            );
            if (exists)
                return "Friendship already Exists.";
            var frnd = new Friends()
            {
                UserId = command.model.FriendId,
                FriendId = command.model.UserId,
                Status = "online",
                FriendName = command.model.UserName,
                UserName = command.model.FriendName              
            };
            var frnd2 = new Friends()
            {
                UserId = command.model.UserId,
                FriendId = command.model.FriendId,
                Status = "online",
                FriendName = command.model.FriendName,
                UserName = command.model.UserName
            };
            await _context.Friends.AddRangeAsync(frnd, frnd2);
            await _context.SaveChangesAsync();

            var accepted = await _context.FriendRequests.Where(x => x.UserId == command.model.FriendId && 
                                 x.ToUserId == command.model.UserId && x.Status == "Pending").FirstOrDefaultAsync();
            if (accepted != null)
            {
                accepted.Status = "Accepted";
                _context.FriendRequests.Update(accepted);
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            return "Friend Request Accepted.";

        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return $"Failed to accept friend request due to: {ex.Message}";
        }
    }
}

