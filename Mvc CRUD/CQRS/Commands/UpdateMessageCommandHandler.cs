using FluentResults;
using MediatR;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class UpdateMessageCommandHandler : IRequestHandler<UpdateMessageCommand, Result>
{
    private readonly DataDbContext _context;
    private readonly ILogger<UpdateMessageCommandHandler> _logger;

    public UpdateMessageCommandHandler(DataDbContext context, ILogger<UpdateMessageCommandHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context)); 
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    public async Task<Result> Handle(UpdateMessageCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(command.model.UserId) || string.IsNullOrEmpty(command.model.ToUserId)) 
        {
            _logger.LogError("User Id or to userId cannot be null/empty");
            return Result.Fail("User Id or to userId cannot be null/empty");
        }

        try
        {
            _context.Chats.Update(command.model);
            await _context.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex) 
        {
            _logger.LogError($"Failed to update due to : {ex.Message}");
            return Result.Fail("Failed to update due to a technical issue.");
        }       
    }
}

