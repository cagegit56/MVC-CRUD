using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Queries;

internal sealed class ReceivedFriendRequestQueryHandler : IRequestHandler<ReceivedFriendRequestQuery, Result<PaginateResponse<List<FriendRequest>>>>
{
    private readonly DataDbContext _context;
    private readonly IUserInfoContextService _currentUser;
    private readonly IPaginationService _pagination;
    private readonly ILogger<ReceivedFriendRequestQueryHandler> _logger;

    public ReceivedFriendRequestQueryHandler(DataDbContext context, IUserInfoContextService currentUser, IPaginationService pagination, ILogger<ReceivedFriendRequestQueryHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _pagination = pagination ?? throw new ArgumentNullException(nameof(pagination));
        _logger = logger;
    }
    public async Task<Result<PaginateResponse<List<FriendRequest>>>> Handle(ReceivedFriendRequestQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var res = await _context.FriendRequests.Where(x => x.ToUserId == _currentUser.UserId 
                            && x.Status == "Pending" && x.isDeleted != true).AsNoTracking().ToListAsync();
            var paginatedRes = await _pagination.Paginate(res, request.pgFilter);
            return Result.Ok(paginatedRes);
        }
        catch (Exception ex) 
        {
            _logger.LogError($"Failed to return all sent friend request due to {ex.Message}");
            return Result.Ok(new PaginateResponse<List<FriendRequest>>()
            {
                Error = "Failed to return all sent friend request see inner exception for more info."
            });
        }      
    }
}

