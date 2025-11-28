using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Commands;

    internal sealed class SendFriendRequestCommandHandler : IRequestHandler<SendFriendRequestCommand, string>
    {
       private readonly DataDbContext _context;

       public SendFriendRequestCommandHandler(DataDbContext context)
        {
           _context = context ?? throw new ArgumentNullException(nameof(context));
        }   

       public async Task<string> Handle(SendFriendRequestCommand command, CancellationToken cancellationToken)
        {
           using var DbTransaction = await _context.Database.BeginTransactionAsync();
           try
            {              
                var exists = await _context.FriendRequests.Where(x => x.UserId == command.model.UserId && x.ToUserId == command.model.ToUserId ||
                              x.UserId == command.model.ToUserId && x.ToUserId == command.model.UserId).FirstOrDefaultAsync();
                if (exists != null)
                _context.FriendRequests.Remove(exists);
                await _context.FriendRequests.AddAsync(command.model);
                await _context.SaveChangesAsync();
                await DbTransaction.CommitAsync();
                return "SuccessFully Sent";
            }
            catch(Exception ex)
            {
                await DbTransaction.RollbackAsync();
                return $"Failed to send a friend request due to : {ex.Message}";
            }
        }
    }

