using FluentResults;
using MediatR;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class BlockUserCommandHandler : IRequestHandler<BlockUserCommand, Result<string>>
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
    public async Task<Result<string>> Handle(BlockUserCommand command, CancellationToken cancellationToken)
    {
        if (command.model.BlockUserId == null && command.model.BlockUserName == null) 
            return Result.Fail("userid and username cannot be null.");
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
                return Result.Fail("current userid and username cannot be null.");
            }

            //await _context.AddAsync(command.model);
            //await _context.SaveChangesAsync(cancellationToken);
            return Result.Ok("SuccessFully blocked.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to save data due to : {ex.Message}");
            return Result.Fail("Failed to block user, Please check the inner exception for more info.");
        }
    }
}

