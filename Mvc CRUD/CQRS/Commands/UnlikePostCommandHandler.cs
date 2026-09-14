using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class UnlikePostCommandHandler : IRequestHandler<UnlikePostCommand, Result>
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
    public async Task<Result> Handle(UnlikePostCommand request, CancellationToken cancellationToken)
    {
        if (request.postId == 0)
        {
            _logger.LogError("Post Id cannot be null/empty.");
            return Result.Fail("Post Id cannot be null/empty.");
        }

        try
        {
            var existingRecord = await _context.Like.Where(x => x.PostId == request.postId 
                     && x.Username == _currentUser.UserName && x.IsDeleted == false).FirstOrDefaultAsync();
            if (existingRecord == null) return Result.Fail("No matching record/like found");

            existingRecord.IsDeleted = true;
            _context.Update(existingRecord);
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
        catch (Exception ex) {
            _logger.LogError($"Failed to unlike a post due to : {ex.Message}");
            return Result.Fail("Failed to unlike a post due to a technical issue.");
        }
    }
}

