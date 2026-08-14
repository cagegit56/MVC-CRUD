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
                var rec = await _context.FriendRequests
                            .Where(x => (x.UserId == _currentUser.UserId && x.ToUserId == command.toUserId)
                            || (x.UserId == command.toUserId && x.ToUserId == _currentUser.UserId)
                            && !x.isDeleted).ToListAsync();

                foreach(var request  in rec) 
                {
                    if(request.UserId == _currentUser.UserId)
                    {
                        request.Status = "Cancelled";
                    }else {
                        request.Status = "Rejected";
                    }
                }           

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

