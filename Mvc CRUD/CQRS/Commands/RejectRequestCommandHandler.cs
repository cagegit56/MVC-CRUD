using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Commands;

    internal sealed class RejectRequestCommandHandler : IRequestHandler<RejectRequestCommand, Result>
    {
       private readonly DataDbContext _context;
       private readonly IUserInfoContextService _currentUser;
       private readonly ILogger<RejectRequestCommandHandler> _logger;

       public RejectRequestCommandHandler(DataDbContext context, IUserInfoContextService currentUser, ILogger<RejectRequestCommandHandler> logger)
        {
           _context = context ?? throw new ArgumentNullException(nameof(context));
           _currentUser = currentUser ?? throw new ArgumentNullException(nameof(_currentUser));
           _logger = logger;
        }
       public async Task<Result> Handle(RejectRequestCommand command, CancellationToken cancellationToken)
       {
          if (string.IsNullOrEmpty(command.toUserId))
              return Result.Fail("ToUserId cannot be null.");
          try
          {
            var res = await _context.FriendRequests.Where(x => 
                            ((x.UserId == _currentUser.UserId && x.ToUserId == command.toUserId) 
                            || (x.UserId == command.toUserId && x.ToUserId == _currentUser.UserId)) 
                            && !x.isDeleted )
                .ExecuteUpdateAsync(u => u.SetProperty(p => p.isDeleted, true));

            if (res == 0) return Result.Fail("No friend request found.");

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Ok();
          }
          catch (Exception ex) 
          {
            _logger.LogError($"Failed to reject friend request due to : {ex.Message}");
            return Result.Fail("Failed to reject friend request, Please see the inner exception for more info.");
          }
       }
    }

