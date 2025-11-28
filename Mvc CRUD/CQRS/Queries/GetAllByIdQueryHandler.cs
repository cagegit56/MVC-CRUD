using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Common;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Queries;

internal sealed class GetAllByIdQueryHandler : IRequestHandler<GetAllByIdQuery, Chat>
{
    private readonly DataDbContext _context;
    private readonly IPaginationService _pagination;

    public GetAllByIdQueryHandler(DataDbContext context, IPaginationService pagination)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _pagination = pagination ?? throw new ArgumentNullException(nameof(pagination));
    }
    public async Task<Chat> Handle(GetAllByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.Id == 0 || request?.Id == null)
                throw new Exception("Id is null/empty");
            var res = await _context.Chats.FirstOrDefaultAsync(x => x.Id == request.Id);
            return res;
        }
        catch (Exception ex) 
        {
            throw new Exception($"Failed to get data due to : {ex.Message}");
        }

    }
}

