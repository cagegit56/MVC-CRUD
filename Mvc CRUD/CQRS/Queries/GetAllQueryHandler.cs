using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Mvc_CRUD.Common;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;
using Mvc_CRUD.Services;
using System.Security.Claims;

namespace Mvc_CRUD.CQRS.Queries;

internal sealed class GetAllQueryHandler : IRequestHandler<GetAllQuery, PaginateResponse<List<Chat>>>
{
    private readonly DataDbContext _context;
    private readonly IPaginationService _pagination;

    public GetAllQueryHandler(DataDbContext context, IPaginationService pagination)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _pagination = pagination ?? throw new ArgumentNullException(nameof(pagination));
    }

    public async Task<PaginateResponse<List<Chat>>> Handle(GetAllQuery request, CancellationToken cancellationToken)
    {
        var res = await _context.Chats.AsNoTracking().ToListAsync() ?? throw new NotFoundException("No Data Found.");
        var paginatedRes = await _pagination.Paginate(res, request.pgFilter);
        return paginatedRes;
    }

}

