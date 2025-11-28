using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Commands;

    internal sealed class RejectRequestCommandHandler : IRequestHandler<RejectRequestCommand, string>
    {
       private readonly DataDbContext _context;

       public RejectRequestCommandHandler(DataDbContext context)
        {
           _context = context ?? throw new ArgumentNullException(nameof(context));
        }
       public async Task<string> Handle(RejectRequestCommand command, CancellationToken cancellationToken)
        {
        try
        {
            var exists = await _context.FriendRequests.Where(x => x.UserId == command.userId && x.ToUserId == command.toUserId ||
                            x.UserId == command.toUserId && x.ToUserId == command.userId).FirstOrDefaultAsync();
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

