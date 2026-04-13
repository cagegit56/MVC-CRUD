using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class UpdateLikesCommandHandler : IRequestHandler<UpdateLikesCommand, bool>
{
    private readonly DataDbContext _context;

    public UpdateLikesCommandHandler(DataDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context)); 
    }

    public async Task<bool> Handle(UpdateLikesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var res = await _context.Like.Where(x => x.PostId == request.postId && x.Username == request.Username).FirstOrDefaultAsync();
            if (res != null)
            {
                res.IsDeleted = false;
                _context.Update(res);
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            else
            {
                throw new Exception($"Falied to re-like");
            }
        }catch(Exception ex)
        {
            throw new Exception($"Falied to re-like due to : {ex.Message} ");
        }
    
       
    }
}

