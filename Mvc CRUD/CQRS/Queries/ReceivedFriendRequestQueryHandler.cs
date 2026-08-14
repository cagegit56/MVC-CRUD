using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Dto;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Queries;

internal sealed class ReceivedFriendRequestQueryHandler : IRequestHandler<ReceivedFriendRequestQuery, Result<PaginateResponse<List<FriendRequestDto>>>>
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
    public async Task<Result<PaginateResponse<List<FriendRequestDto>>>> Handle(ReceivedFriendRequestQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return Result.Ok( await _pagination.PaginateAndMap<FriendRequest, FriendRequestDto>(
                            _context.FriendRequests.Where(x => x.ToUserId == _currentUser.UserId 
                            && x.Status == "Pending" && !x.isDeleted).AsNoTracking(), request.pgFilter));
        }
        catch (Exception ex) 
        {
            _logger.LogError($"Failed to return all sent friend request due to {ex.Message}");
            return Result.Ok(new PaginateResponse<List<FriendRequestDto>>()
            {
                Error = "Failed to return all sent friend request see inner exception for more info."
            });
        }      
    }
}

