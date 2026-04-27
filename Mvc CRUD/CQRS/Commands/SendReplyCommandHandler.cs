using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class SendReplyCommandHandler : IRequestHandler<SendReplyCommand, bool>
{
    private readonly DataDbContext _context;

    public SendReplyCommandHandler(DataDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<bool> Handle(SendReplyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _context.ReplyComments.AddAsync(request.model);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to send a reply due to : {ex.Message}");
        }
    }
}

