using MediatR;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class BlockUserCommandHandler : IRequestHandler<BlockUserCommand, string>
{
    private readonly DataDbContext _context;
    public BlockUserCommandHandler(DataDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    public async Task<string> Handle(BlockUserCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var res = await _context.BlockedUser.AddAsync(command.model);
            await _context.SaveChangesAsync(cancellationToken);
            return "Successfully Saved";
        }
        catch (Exception ex)
        {
            return $"Failed to save data due to : {ex.Message}";
        }
    }
}

