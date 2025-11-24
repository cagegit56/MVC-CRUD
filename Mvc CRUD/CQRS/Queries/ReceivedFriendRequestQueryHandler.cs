using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Queries;

internal sealed class ReceivedFriendRequestQueryHandler : IRequestHandler<ReceivedFriendRequestQuery, PaginateResponse<List<FriendRequest>>>
{
    private readonly DataDbContext _context;
    private readonly IPaginationService _pagination;

    public ReceivedFriendRequestQueryHandler(DataDbContext context, IPaginationService pagination)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _pagination = pagination ?? throw new ArgumentNullException(nameof(pagination));
    }
    public async Task<PaginateResponse<List<FriendRequest>>> Handle(ReceivedFriendRequestQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var res = await _context.FriendRequests.Where(x => x.ToUserId == request.UserId && x.Status == "Pending" && x.isDeleted != true).ToListAsync();
            var paginatedRes = await _pagination.Paginate(res, request.pgFilter);
            return paginatedRes;
        }
        catch (Exception ex) 
        {
            throw new Exception($"{ex.Message}");
        }
      
    }
}

