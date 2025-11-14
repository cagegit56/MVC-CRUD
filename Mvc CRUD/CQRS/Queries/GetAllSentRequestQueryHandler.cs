using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Queries;

    internal sealed class GetAllSentRequestQueryHandler :IRequestHandler<GetAllSentRequestQuery, PaginateResponse<List<FriendRequest>>>
    {
        private readonly DataDbContext _context;
        private readonly IPaginationService _pagination;
        public GetAllSentRequestQueryHandler(DataDbContext context, IPaginationService pagination)
        {
           _context = context ?? throw new ArgumentNullException(nameof(context));
           _pagination = pagination ?? throw new ArgumentNullException(nameof(pagination));
        }
        public async Task<PaginateResponse<List<FriendRequest>>> Handle(GetAllSentRequestQuery request, CancellationToken cancellationToken)
        {
            var res = await _context.FriendRequests.Where(x => x.UserId == request.userId && x.Status != "Accepted" && x.isDeleted != true).ToListAsync();
            var paginatedRes = await _pagination.Paginate(res, request.pgFilter);
            return paginatedRes;
        }
    }

