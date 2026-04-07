using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class UnlikePostCommandHandler : IRequestHandler<UnlikePostCommand, bool>
{
    private readonly DataDbContext _context;
    public UnlikePostCommandHandler(DataDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    public async Task<bool> Handle(UnlikePostCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existingRecord = await _context.Like.Where(x => x.PostId == request.postId &&
                                   x.Username == request.userName).FirstOrDefaultAsync();
            if (existingRecord == null) return false;
            existingRecord.IsDeleted = true;
            _context.Update(existingRecord);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex) {
            throw new Exception(ex.Message);
        }
    }
}

