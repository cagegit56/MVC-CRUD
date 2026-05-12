using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class ReLikeCommandHandler : IRequestHandler<ReLikeCommand, bool>
{
    private readonly DataDbContext _context;
    private readonly ILogger<ReLikeCommandHandler> _logger;

    public ReLikeCommandHandler(DataDbContext context, ILogger<ReLikeCommandHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger;
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
                _logger.LogError("Cannot re-like a like that does not exist");
                return false;
            }
        }catch(Exception ex)
        {
            _logger.LogError($"Falied to re-like due to : {ex.Message} ");
            return false;
        }
    
       
    }
}

