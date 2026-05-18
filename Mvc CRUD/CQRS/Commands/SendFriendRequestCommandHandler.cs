using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.CQRS.Queries;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Commands;

    internal sealed class SendFriendRequestCommandHandler : IRequestHandler<SendFriendRequestCommand, bool>
    {
       private readonly DataDbContext _context;
       private readonly IMediator _mediator;
       private readonly ILogger<SendFriendRequestCommandHandler> _logger;

       public SendFriendRequestCommandHandler(DataDbContext context, IMediator mediator, ILogger<SendFriendRequestCommandHandler> logger)
        {
           _context = context ?? throw new ArgumentNullException(nameof(context));
           _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
           _logger = logger;
        }   

       public async Task<bool> Handle(SendFriendRequestCommand command, CancellationToken cancellationToken)
        {
           try
           {
              var currentUser = await _mediator.Send(new GetUserProfileQuery());
              var exists = await _context.FriendRequests.Where(x => x.UserId == currentUser.UserId  
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

              if (currentUser.UserId != null && currentUser.UserName != null && currentUser.LastName != null)
              {
                command.model.UserId = currentUser.UserId;
                command.model.UserName = currentUser.UserName;
                command.model.LastName = currentUser.LastName;
                command.model.ProfilePicUrl = currentUser.UserProfilePicUrl;
              }else{
                _logger.LogError("Current user info cannot be null.");
                return false;
              }
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

