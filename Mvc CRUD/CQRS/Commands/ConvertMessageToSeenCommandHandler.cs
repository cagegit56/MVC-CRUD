using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class ConvertMessageToSeenCommandHandler : IRequestHandler<ConvertMessageToSeenCommand, Result>
{
    private readonly DataDbContext _context;
    private readonly IUserInfoContextService _currentUser;
    private readonly ILogger<ConvertMessageToSeenCommandHandler> _logger;

    public ConvertMessageToSeenCommandHandler(DataDbContext context, IUserInfoContextService currentUser, ILogger<ConvertMessageToSeenCommandHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> Handle(ConvertMessageToSeenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _context.Chats
                .Where(x => (x.UserId == request.userId && x.ToUserId == _currentUser.UserId) && !x.Seen)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Seen, true).SetProperty(t => t.UnreadMsgCount, 0));

            return Result.Ok();
        }
        catch (Exception ex) {
            _logger.LogError($"Failed to convert message to seen due to {ex.Message}.");
            return Result.Fail("Failed to convert message to seen due to a technical error.");
        }
    }
}

