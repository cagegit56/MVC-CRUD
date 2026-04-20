using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class ReLikeCommandHandler : IRequestHandler<ReLikeCommand, bool>
{
    private readonly DataDbContext _context;

    public ReLikeCommandHandler(DataDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context)); 
    }

    public async Task<bool> Handle(ReLikeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var res = await _context.Like.Where(x => x.PostId == request.postId && x.Username == request.Username && x.IsDeleted == true).FirstOrDefaultAsync();
            if (res != null)
            {
                res.IsDeleted = false;
                _context.Update(res);
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            else
            {
                return false;
            }
        }catch(Exception ex)
        {
            throw new Exception($"Falied to re-like due to : {ex.Message} ");
        }
    
       
    }
}

