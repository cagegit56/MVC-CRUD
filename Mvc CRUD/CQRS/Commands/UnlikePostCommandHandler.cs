using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class UnlikePostCommandHandler : IRequestHandler<UnlikePostCommand, bool>
{
    private readonly DataDbContext _context;
    private readonly IUserInfoContextService _currentUser;
    private readonly ILogger<UnlikePostCommandHandler> _logger;
    public UnlikePostCommandHandler(DataDbContext context, IUserInfoContextService currentUser, ILogger<UnlikePostCommandHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _logger = logger;
    }
    public async Task<bool> Handle(UnlikePostCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existingRecord = await _context.Like.Where(x => x.PostId == request.postId 
                                 && x.Username == _currentUser.UserName && x.IsDeleted == false).FirstOrDefaultAsync();
            if (existingRecord == null) return false;
            existingRecord.IsDeleted = true;
            _context.Update(existingRecord);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex) {
            _logger.LogError($"Failed to unlike a post due to : {ex.Message}");
            return false;
        }
    }
}

