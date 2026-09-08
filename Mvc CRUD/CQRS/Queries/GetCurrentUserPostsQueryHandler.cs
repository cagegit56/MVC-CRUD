using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Queries;

internal sealed class GetCurrentUserPostsQueryHandler : IRequestHandler<GetCurrentUserPostsQuery, PaginateResponse<List<Posts>>>
{
    private readonly DataDbContext _context;
    private readonly IUserInfoContextService _currentUser;
    private readonly IPaginationService _paginatation;
    private readonly ILogger<GetCurrentUserPostsQueryHandler> _logger;

    public GetCurrentUserPostsQueryHandler(DataDbContext context, IUserInfoContextService currentUser,
        IPaginationService paginatation, ILogger<GetCurrentUserPostsQueryHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _paginatation = paginatation ?? throw new ArgumentNullException(nameof(paginatation));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _logger = logger;
    }

    public async Task<PaginateResponse<List<Posts>>> Handle(GetCurrentUserPostsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return await _paginatation.Paginate( await _context.Post
                .Where(x => x.UserId == _currentUser.UserId).AsNoTracking()
                .OrderByDescending(x => x.CreatedOn).ToListAsync(),
                request.pgFilter, cancellationToken);
        }
        catch (Exception ex) {
            _logger.LogError($"Failed to return user's posts due to : {ex.Message}");
            return new PaginateResponse<List<Posts>>() { Error = "Failed to return user's posts, Tehnical issue" };
        }
       
    }
}

