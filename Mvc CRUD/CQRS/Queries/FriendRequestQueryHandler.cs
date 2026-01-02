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
            var potentialFriends = await _context.ChatUsers.Where(x => x.UserId != request.UserId)
                                    .Where(x => !_context.Friends
                                        .Any(y => (y.UserId == request.UserId && y.FriendId == x.UserId) ||
                                                  (y.FriendId == request.UserId && y.UserId == x.UserId)))
                                    .Where(x => !_context.BlockedUser
                                        .Any(v => v.UserId == request.UserId && v.BlockUserId == x.UserId))
                                    .Where(x => !_context.FriendRequests
                                        .Any(z => z.Status == "Pending" && (z.UserId == request.UserId && z.ToUserId == x.UserId) || 
                                                   (z.UserId == x.UserId && z.ToUserId == request.UserId)))
                                    .Where(x => x.UserName != request.Username)
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

