using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class SendCommentCommandHandler : IRequestHandler<SendCommentCommand, bool>
{
    private readonly DataDbContext _context;
    private readonly IUserInfoContextService _currentUser;

    public SendCommentCommandHandler(DataDbContext context, IUserInfoContextService currentUser)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<bool> Handle(SendCommentCommand request, CancellationToken cancellationToken)
    {
        using var trans = await _context.Database.BeginTransactionAsync();
        try
        {
            request.model.UserId = _currentUser.UserId!;
            request.model.UserName = _currentUser.UserName!;
            var postInfo = await _context.Post.Where(x => x.Id == request.model.PostId)
                .ExecuteUpdateAsync(p => p.SetProperty(x => x.TotalComments, c => (c.TotalComments ?? 0) + 1));
            await _context.Comment.AddAsync(request.model);
            await _context.SaveChangesAsync();
            await trans.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await trans.RollbackAsync();
            throw new Exception($"Failed to send a comment due to : {ex.Message}");
        }
    }
}

