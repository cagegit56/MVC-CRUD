using MediatR;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class BlockUserCommandHandler : IRequestHandler<BlockUserCommand, bool>
{
    private readonly DataDbContext _context;
    private readonly IUserInfoContextService _currentUser;
    private readonly ILogger<BlockUserCommandHandler> _logger;
    public BlockUserCommandHandler(DataDbContext context, IUserInfoContextService currentUser, ILogger<BlockUserCommandHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _logger = logger;
    }
    public async Task<bool> Handle(BlockUserCommand command, CancellationToken cancellationToken)
    {
        try
        {
            if(_currentUser.UserId != null && _currentUser.UserName != null)
            {
                command.model.UserId = _currentUser.UserId;
                command.model.UserName = _currentUser.UserName;
            }
            else
            {
                _logger.LogError("Current user info cannot be null");
                return false;
            }
            
            var res = await _context.BlockedUser.AddAsync(command.model);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to save data due to : {ex.Message}");
            return false;
        }
    }
}

