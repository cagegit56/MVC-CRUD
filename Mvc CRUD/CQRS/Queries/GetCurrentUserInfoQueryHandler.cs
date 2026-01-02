using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Queries;

internal sealed class GetCurrentUserInfoQueryHandler : IRequestHandler<GetCurrentUserInfoQuery, Chat_Users?>
{
    private readonly DataDbContext _context;

    public GetCurrentUserInfoQueryHandler(DataDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Chat_Users?> Handle(GetCurrentUserInfoQuery request, CancellationToken cancellationToken)
    {
        return await _context.ChatUsers.Where(x => x.UserId == request.UserId).FirstOrDefaultAsync();
    }
}

