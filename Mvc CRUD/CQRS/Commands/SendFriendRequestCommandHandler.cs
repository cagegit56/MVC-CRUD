using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Commands;

    internal sealed class SendFriendRequestCommandHandler : IRequestHandler<SendFriendRequestCommand, bool>
    {
       private readonly DataDbContext _context;
       private readonly IUserInfoContextService _currentUser;
       private readonly ILogger<SendFriendRequestCommandHandler> _logger;

       public SendFriendRequestCommandHandler(DataDbContext context, IUserInfoContextService currentUser, ILogger<SendFriendRequestCommandHandler> logger)
        {
           _context = context ?? throw new ArgumentNullException(nameof(context));
           _currentUser = currentUser ?? throw new ArgumentNullException(nameof(_currentUser));
           _logger = logger;
        }   

       public async Task<bool> Handle(SendFriendRequestCommand command, CancellationToken cancellationToken)
        {
           try
           {
              var exists = await _context.FriendRequests.Where(x => x.UserId == _currentUser.UserId  
                                 && x.ToUserId == command.model.ToUserId).FirstOrDefaultAsync();
              if (exists != null)
              {
                 if (exists.isDeleted)
                 {
                    exists.isDeleted = false;
                    _context.Update(exists);
                    await _context.SaveChangesAsync(cancellationToken);
                 }
                 return true; 
              }

              command.model.UserId = _currentUser.UserId!;
              command.model.UserName = _currentUser.UserName!;
              command.model.LastName = _currentUser.LastName!;
              await _context.AddAsync(command.model);
              await _context.SaveChangesAsync(cancellationToken);
              return true;
           }
           catch(Exception ex)
           {
              _logger.LogError($"Failed to send a friend request from {command.model.UserName} to {command.model.ToUserName} to due to : {ex.Message}");
              return false;
           }
        }
    }

