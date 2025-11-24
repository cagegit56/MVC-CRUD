using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Queries;

internal sealed class FriendRequestQueryHandler : IRequestHandler<FriendRequestQuery, PaginateResponse<List<Chat_Users>>>
{
    private readonly DataDbContext _context;
    private readonly IPaginationService _pagination;

    public FriendRequestQueryHandler(DataDbContext context, IPaginationService pagination)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _pagination = pagination ?? throw new ArgumentNullException(nameof(pagination));
    }
    public async Task<PaginateResponse<List<Chat_Users>>> Handle(FriendRequestQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var friends = await _context.Friends.Where(x => x.UserId == request.UserId || x.FriendId == request.UserId).Select(x => x.UserId).ToListAsync();
            var blockedUser1 = await _context.BlockedUser.Where(x => x.UserId == request.UserId).Select(x => x.BlockUserId).ToListAsync();
            var potentialFriends = await _context.ChatUsers.Where(x => x.UserId != request.UserId && !friends.Contains(x.UserId) && !blockedUser1.Contains(x.UserId)).ToListAsync();
            var paginatedRes = await _pagination.Paginate(potentialFriends, request.pgFilter);
            return paginatedRes;
        }
        catch (Exception ex) 
        {
            throw new Exception($"Failed Due to {ex.Message}");
        }
    }
}

