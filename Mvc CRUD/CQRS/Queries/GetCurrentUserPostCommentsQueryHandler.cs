using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Queries;

internal sealed class GetCurrentUserPostCommentsQueryHandler : IRequestHandler<GetCurrentUserPostCommentsQuery, List<Comments>>
{
    private readonly DataDbContext _context;
    private readonly ILogger<GetCurrentUserPostCommentsQueryHandler> _logger;

    public GetCurrentUserPostCommentsQueryHandler(DataDbContext context, ILogger<GetCurrentUserPostCommentsQueryHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger;
    }

    public async Task<List<Comments>> Handle(GetCurrentUserPostCommentsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var res = await _context.Comment.Where(x => x.PostId == request.postId).AsNoTracking()
                .OrderByDescending(x => x.SentOn).ToListAsync();
            return res;
        }
        catch (Exception ex) {
            _logger.LogError($"Failed to retrieve user's post comments due to : {ex.Message}");
            throw new Exception($"Failed to retrieve user's post comments due to : {ex.Message}");
        }
    }
}

