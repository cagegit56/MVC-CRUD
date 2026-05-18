using MediatR;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class SendReplyOfReplyCommandHandler : IRequestHandler<SendReplyOfReplyCommand, bool>
{
    private readonly DataDbContext _context;
    private readonly ILogger<SendReplyOfReplyCommandHandler> _logger;

    public SendReplyOfReplyCommandHandler(DataDbContext context, ILogger<SendReplyOfReplyCommandHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger;
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
            _logger.LogError($"Failed to reply due to : {ex.Message}"); 
            return false;
        }
    }
}
