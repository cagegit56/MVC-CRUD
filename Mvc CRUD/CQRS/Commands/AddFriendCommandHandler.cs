using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;
using System.Net.NetworkInformation;
using System.Security.Claims;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class AddFriendCommandHandler : IRequestHandler<AddFriendCommand, bool>
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

    public async Task<bool> Handle(AddFriendCommand command, CancellationToken cancellationToken)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            bool exists = await _context.Friends.AnyAsync(x =>
                (x.UserId == _currentUser.UserId && x.FriendId == command.model.FriendId) ||
                (x.UserId == command.model.FriendId && x.FriendId == _currentUser.UserId)
            );
            if (exists) return true;

            var sender = new Friends();
            var reciever = new Friends();
            if (_currentUser.UserId != null && _currentUser.UserName != null)
            {
                sender.UserId = _currentUser.UserId;
                sender.FriendId = command.model.FriendId;
                sender.Status = "online";
                sender.FriendName = command.model.FriendName;
                sender.UserName = _currentUser.UserName;

                reciever.UserId = command.model.FriendId;
                reciever.FriendId = _currentUser.UserId;
                reciever.Status = "online";
                reciever.FriendName = _currentUser.UserName;
                reciever.UserName = command.model.FriendName;
            }
            else
            {
                _logger.LogError("Current user info cannot be null.");
                return false;
            }

            await _context.Friends.AddRangeAsync(reciever, sender);
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
            _logger.LogError($"Failed to accept friend request due to: {ex.Message}");
            return false;
        }
    }
}

