using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Queries;

internal sealed class GetAllPostsQueryHandler : IRequestHandler<GetAllPostsQuery, List<Posts>>
{
    private readonly DataDbContext _context;
    public GetAllPostsQueryHandler(DataDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    public async Task<List<Posts>> Handle(GetAllPostsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var allowedScope = new[] { "Public", "Friends", "Only me" };
            var res = await _context.Post.Where(p => allowedScope.Contains(p.PostScope)).OrderByDescending(x => x.CreatedOn).AsNoTracking()
                                         .ToListAsync(cancellationToken);
            return res;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to retrieve posts due to : {ex.Message} who is it");
        }
    }
}

