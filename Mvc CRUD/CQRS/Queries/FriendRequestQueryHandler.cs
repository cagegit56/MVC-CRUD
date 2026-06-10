using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Queries;

internal sealed class FriendRequestQueryHandler : IRequestHandler<FriendRequestQuery, Result<PaginateResponse<List<UserProfile>>>>
{
    private readonly DataDbContext _context;
    private readonly IUserInfoContextService _currentUser;
    private readonly IPaginationService _pagination;
    private readonly ILogger<FriendRequestQueryHandler> _logger;

    public FriendRequestQueryHandler(DataDbContext context, IUserInfoContextService currentUser, 
        IPaginationService pagination, 
        ILogger<FriendRequestQueryHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _pagination = pagination ?? throw new ArgumentNullException(nameof(pagination));
        _logger = logger;
    }
    public async Task<Result<PaginateResponse<List<UserProfile>>>> Handle(FriendRequestQuery request, CancellationToken cancellationToken)
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
                                    .AsSplitQuery().AsNoTracking().ToListAsync();
            request.pgFilter.PageNumber = 1;
            request.pgFilter.PageSize = 5;
            var paginatedRes = await _pagination.Paginate(potentialFriends, request.pgFilter);
            return Result.Ok(paginatedRes);
        }
        catch (Exception ex) 
        {
            var res = new PaginateResponse<List<UserProfile>>()
            {
                Error = "Failed to return all potential friends."
            };

            _logger.LogError($"Failed Due to {ex.Message}");
            return Result.Ok(res);
        }
    }
}

