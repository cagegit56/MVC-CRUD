using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class SendReplyCommandHandler : IRequestHandler<SendReplyCommand, Result>
{
    private readonly DataDbContext _context;
    private readonly ILogger<SendReplyCommandHandler> _logger;

    public SendReplyCommandHandler(DataDbContext context, ILogger<SendReplyCommandHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger;
    }

    public async Task<Result> Handle(SendReplyCommand request, CancellationToken cancellationToken)
    {
        if (request.model.CommentId == 0)
        {
            _logger.LogError("Comment Id cannot be null/empty.");
            return Result.Fail("Comment Id cannot be null/empty.");
        }
            
        try
        {
            await _context.ReplyComments.AddAsync(request.model);
            await _context.Comment.Where(x => x.Id == request.model.CommentId)
                .ExecuteUpdateAsync(d => d.SetProperty(k => k.TotalCommentReplies, p => p.TotalCommentReplies + 1));
            await _context.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to send a reply due to : {ex.Message}");
            return Result.Fail("Failed to send a reply due to a technical issue.");
        }
    }
}

