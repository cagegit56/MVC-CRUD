using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class ReLikeCommandHandler : IRequestHandler<ReLikeCommand, Result>
{
    private readonly DataDbContext _context;
    private readonly ILogger<ReLikeCommandHandler> _logger;

    public ReLikeCommandHandler(DataDbContext context, ILogger<ReLikeCommandHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger;
    }

    public async Task<Result> Handle(ReLikeCommand request, CancellationToken cancellationToken)
    {
        if (request.postId == 0)
        {
            _logger.LogError("Post Id cannot be null.");
            return Result.Fail("Post Id cannot be null.");
        }
           
        try
        {
            var res = await _context.Like
                .Where(x => (x.PostId == request.postId && x.Username == request.userName)
                && x.IsDeleted == true).FirstOrDefaultAsync();
            if (res != null)
            {
                res.IsDeleted = false;
                _context.Update(res);
                await _context.SaveChangesAsync(cancellationToken);
                return Result.Ok();
            }
            else
            {
                _logger.LogError("Cannot re-like a like that does not exist");
                return Result.Fail("Cannot re-like a like that does not exist");
            }
        }catch(Exception ex)
        {
            _logger.LogError($"Falied to re-like due to : {ex.Message} ");
            return Result.Fail("Falied to re-like due to a technical issue.");
        }       
    }
}

