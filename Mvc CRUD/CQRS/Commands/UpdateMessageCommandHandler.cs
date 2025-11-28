using MediatR;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class UpdateMessageCommandHandler : IRequestHandler<UpdateMessageCommand, string>
{
    private readonly DataDbContext _context;

    public UpdateMessageCommandHandler(DataDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context)); 
    }
    public async Task<string> Handle(UpdateMessageCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var res = _context.Chats.Update(command.model);
            await _context.SaveChangesAsync();
            return "Updated Successfully";
        }
        catch (Exception ex) 
        {
            return $"Failed to update due to : {ex.Message}";
        }
       
    }
}

