using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Queries;

internal sealed class GetCurrentUserPostCommentsQueryHandler : IRequestHandler<GetCurrentUserPostCommentsQuery, PaginateResponse<List<Comments>>>
{
    private readonly DataDbContext _context;
    private readonly IPaginationService _pagination;
    private readonly ILogger<GetCurrentUserPostCommentsQueryHandler> _logger;

    public GetCurrentUserPostCommentsQueryHandler(DataDbContext context, IPaginationService pagination, ILogger<GetCurrentUserPostCommentsQueryHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _pagination = pagination ?? throw new ArgumentNullException(nameof(pagination));
        _logger = logger;
    }

    public async Task<PaginateResponse<List<Comments>>> Handle(GetCurrentUserPostCommentsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var res = await _context.Comment.Where(x => x.PostId == request.postId).AsNoTracking()
                .OrderByDescending(x => x.SentOn).ToListAsync();
            var paginatedRes = await _pagination.Paginate(res, request.pgFilter, cancellationToken);
            return paginatedRes;
        }
        catch (Exception ex) {
            _logger.LogError($"Failed to retrieve user's post comments due to : {ex.Message}");
            return new PaginateResponse<List<Comments>>() { Error = "Failed to retrieve user's post comments technical issue." };
        }
    }
}

