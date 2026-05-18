using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Queries;

internal sealed class FriendRequestQueryHandler : IRequestHandler<FriendRequestQuery, PaginateResponse<List<UserProfile>>>
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
    public async Task<PaginateResponse<List<UserProfile>>> Handle(FriendRequestQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var potentialFriends = await _context.Profile.Where(p => p.UserId != _currentUser.UserId)
                                    .Where(p => !_context.Friends
                                        .Any(f => (f.UserId == _currentUser.UserId && f.FriendId == p.UserId) ||
                                                  (f.FriendId == _currentUser.UserId && f.UserId == p.UserId)))
                                    .Where(p => !_context.BlockedUser
                                        .Any(b => b.UserId == _currentUser.UserId && b.BlockUserId == p.UserId))
                                    .Where(p => !_context.FriendRequests
                                        .Any(r => (r.UserId == _currentUser.UserId && r.ToUserId == p.UserId) && (r.Status == "Pending" && r.isDeleted == false)))
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

