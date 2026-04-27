using MediatR;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class SendReplyOfReplyCommandHandler : IRequestHandler<SendReplyOfReplyCommand, bool>
{
    private readonly DataDbContext _context;

    public SendReplyOfReplyCommandHandler(DataDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<bool> Handle(SendReplyOfReplyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _context.Replies.AddAsync(request.model);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex) 
        {
            throw new Exception($"Failed to reply due to : {ex.Message}"); 
        }
    }
}
