using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class SendReplyOfReplyCommandHandler : IRequestHandler<SendReplyOfReplyCommand, Result>
{
    private readonly DataDbContext _context;
    private readonly ILogger<SendReplyOfReplyCommandHandler> _logger;

    public SendReplyOfReplyCommandHandler(DataDbContext context, ILogger<SendReplyOfReplyCommandHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger;
    }

    public async Task<Result> Handle(SendReplyOfReplyCommand request, CancellationToken cancellationToken)
    {
        if (request.model.ReplyId == 0)
        {
            _logger.LogError("Reply Id cannot be null/empty.");
            return Result.Fail("Reply Id cannot be null/empty.");
        }
            
        try
        {
            await _context.Replies.AddAsync(request.model);
            await _context.ReplyComments.Where(x => x.Id == request.model.ReplyId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.TotalReplies, d => d.TotalReplies + 1));
            await _context.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex) 
        {
            _logger.LogError($"Failed to send a reply due to : {ex.Message}"); 
            return Result.Fail("Failed to send a reply due to technical issue.");
        }
    }
}
