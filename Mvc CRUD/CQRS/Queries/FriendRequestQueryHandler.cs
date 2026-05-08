using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Queries;

internal sealed class FriendRequestQueryHandler : IRequestHandler<FriendRequestQuery, PaginateResponse<List<Chat_Users>>>
{
    private readonly DataDbContext _context;
    private readonly IUserInfoContextService _currentUser;
    private readonly IPaginationService _pagination;

    public FriendRequestQueryHandler(DataDbContext context, IUserInfoContextService currentUser, IPaginationService pagination)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _pagination = pagination ?? throw new ArgumentNullException(nameof(pagination));
    }
    public async Task<PaginateResponse<List<Chat_Users>>> Handle(FriendRequestQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var potentialFriends = await _context.ChatUsers.Where(x => x.UserId != _currentUser.UserId)
                                    .Where(x => !_context.Friends
                                        .Any(y => (y.UserId == _currentUser.UserId && y.FriendId == x.UserId) ||
                                                  (y.FriendId == _currentUser.UserId && y.UserId == x.UserId)))
                                    .Where(x => !_context.BlockedUser
                                        .Any(v => v.UserId == _currentUser.UserId && v.BlockUserId == x.UserId))
                                    .Where(x => !_context.FriendRequests
                                        .Any(z => (z.UserId == _currentUser.UserId && z.ToUserId == x.UserId) && (z.Status == "Pending" && z.isDeleted == false)))
                                    .ToListAsync();
            var paginatedRes = await _pagination.Paginate(potentialFriends, request.pgFilter);
            return paginatedRes;
        }
        catch (Exception ex) 
        {
            throw new Exception($"Failed Due to {ex.Message}");
        }
    }
}

