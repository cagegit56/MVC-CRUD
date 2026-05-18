using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Queries;

internal sealed class GetCurrentUserPostsQueryHandler : IRequestHandler<GetCurrentUserPostsQuery, List<Posts>>
{
    private readonly DataDbContext _context;
    private readonly IUserInfoContextService _currentUser;
    private readonly ILogger<GetCurrentUserPostsQueryHandler> _logger;

    public GetCurrentUserPostsQueryHandler(DataDbContext context, IUserInfoContextService currentUser, ILogger<GetCurrentUserPostsQueryHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _logger = logger;
    }

    public async Task<List<Posts>> Handle(GetCurrentUserPostsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var res = await _context.Post.Where(x => x.UserId == _currentUser.UserId)
                             .AsNoTracking().OrderByDescending(x => x.CreatedOn).ToListAsync();
            return res;
        }
        catch (Exception ex) {
            _logger.LogError($"Failed to return user's posts due to : {ex.Message}");
            throw new Exception($"Failed to return user's posts due to : {ex.Message}");
        }
       
    }
}

