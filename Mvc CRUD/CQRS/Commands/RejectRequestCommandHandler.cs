using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Commands;

    internal sealed class RejectRequestCommandHandler : IRequestHandler<RejectRequestCommand, string>
    {
       private readonly DataDbContext _context;
       private readonly IUserInfoContextService _currentUser;

       public RejectRequestCommandHandler(DataDbContext context, IUserInfoContextService currentUser)
        {
           _context = context ?? throw new ArgumentNullException(nameof(context));
           _currentUser = currentUser ?? throw new ArgumentNullException(nameof(_currentUser));
        }
       public async Task<string> Handle(RejectRequestCommand command, CancellationToken cancellationToken)
        {
        try
        {
            var exists = await _context.FriendRequests.Where(x => x.UserId == _currentUser.UserId && x.ToUserId == command.toUserId ||
                            x.UserId == command.toUserId && x.ToUserId == _currentUser.UserId).FirstOrDefaultAsync();
            if (exists != null)
            {
                exists.isDeleted = true;
                _context.FriendRequests.Update(exists);
                await _context.SaveChangesAsync();
                return "Successfully Cancelled";
            }
            else
            {
                return "UserId or FriendId is null or does not exist";
            }

        }
        catch (Exception ex) 
        {
            return $"Failed To Cancel or Reject request due to : {ex.Message}";
        }


        }
    }

