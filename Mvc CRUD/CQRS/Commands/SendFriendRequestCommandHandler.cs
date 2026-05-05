using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Commands;

    internal sealed class SendFriendRequestCommandHandler : IRequestHandler<SendFriendRequestCommand, bool>
    {
       private readonly DataDbContext _context;
       private readonly IUserInfoContextService _currentUser;

       public SendFriendRequestCommandHandler(DataDbContext context, IUserInfoContextService currentUser)
        {
           _context = context ?? throw new ArgumentNullException(nameof(context));
           _currentUser = currentUser ?? throw new ArgumentNullException(nameof(_currentUser));
        }   

       public async Task<bool> Handle(SendFriendRequestCommand command, CancellationToken cancellationToken)
        {
           try
            {
                var exists = await _context.FriendRequests.AnyAsync(x => x.UserId == _currentUser.UserId && x.ToUserId == command.model.ToUserId);
                if (exists) return true;
                command.model.UserId = _currentUser.UserId!;
                command.model.UserName = _currentUser.UserName!;
                await _context.AddAsync(command.model);
                await _context.SaveChangesAsync(cancellationToken);
            return true;
            }
            catch(Exception ex)
            {
                throw new Exception($"Failed to send a friend request due to : {ex.Message}");
            }
        }
    }

