using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class LikeCommandHandler : IRequestHandler<LikeCommand, bool>
{
    private readonly DataDbContext _context;
    private readonly IMediator _mediator;
    public LikeCommandHandler(DataDbContext context, IMediator mediator)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mediator = mediator;
    }

    public async Task<bool> Handle(LikeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var checkExistence = await _context.Like.Where(x => x.PostId == request.model.PostId &&
                                  x.Username == request.model.Username).FirstOrDefaultAsync();
            if (checkExistence != null)
            {
                bool res = false;
                if(checkExistence.IsDeleted)
                {
                    res = await _mediator.Send(new UpdateLikesCommand(checkExistence.PostId, checkExistence.Username)); 
                }
                return res;
            }
            else
            {
                await _context.AddAsync(request.model, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
           
        }
        catch (Exception ex) {
            throw new Exception(ex.Message);
        }
    }
}

