using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Commands;

    internal sealed class RejectRequestCommandHandler : IRequestHandler<RejectRequestCommand, bool>
    {
       private readonly DataDbContext _context;
       private readonly IUserInfoContextService _currentUser;

       public RejectRequestCommandHandler(DataDbContext context, IUserInfoContextService currentUser)
        {
           _context = context ?? throw new ArgumentNullException(nameof(context));
           _currentUser = currentUser ?? throw new ArgumentNullException(nameof(_currentUser));
        }
       public async Task<bool> Handle(RejectRequestCommand command, CancellationToken cancellationToken)
       {
         try
         {
            var exists = await _context.FriendRequests.Where(x => x.UserId == _currentUser.UserId && x.ToUserId == command.toUserId ||
                            x.UserId == command.toUserId && x.ToUserId == _currentUser.UserId).FirstOrDefaultAsync();
            if (exists == null) return false;
            exists.isDeleted = true;
            _context.FriendRequests.Update(exists);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
         }
         catch (Exception ex) 
         {
            throw new Exception($"Failed To Cancel or Reject request due to : {ex.Message}");
         }
       }
    }

