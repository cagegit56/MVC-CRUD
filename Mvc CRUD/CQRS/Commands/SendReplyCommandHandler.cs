using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class SendReplyCommandHandler : IRequestHandler<SendReplyCommand, bool>
{
    private readonly DataDbContext _context;
    private readonly ILogger<SendReplyCommandHandler> _logger;

    public SendReplyCommandHandler(DataDbContext context, ILogger<SendReplyCommandHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger;
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
            _logger.LogError($"Failed to send a reply due to : {ex.Message}");
            return false;
        }
    }
}

