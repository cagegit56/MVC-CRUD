using MediatR;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class BlockUserCommandHandler : IRequestHandler<BlockUserCommand, bool>
{
    private readonly DataDbContext _context;
    private readonly IUserInfoContextService _currentUser;
    public BlockUserCommandHandler(DataDbContext context, IUserInfoContextService currentUser)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }
    public async Task<bool> Handle(BlockUserCommand command, CancellationToken cancellationToken)
    {
        try
        {
            command.model.UserId = _currentUser.UserId!;
            command.model.UserName = _currentUser.UserName!;
            var res = await _context.BlockedUser.AddAsync(command.model);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to save data due to : {ex.Message}");
        }
    }
}

