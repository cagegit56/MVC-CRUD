using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Queries;

internal sealed class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfile?>
{
    private readonly DataDbContext _context;

    public GetUserProfileQueryHandler(DataDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<UserProfile?> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var res = await _context.Profile.Where(x => x.UserId == request.userId).FirstOrDefaultAsync();
            return res;
        }
        catch (Exception ex) 
        {
            throw new Exception($"Failed to return data due to : {ex.Message}");
        }

    }
}

