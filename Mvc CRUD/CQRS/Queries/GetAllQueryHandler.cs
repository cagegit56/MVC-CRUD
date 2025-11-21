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
    private readonly IMemoryCache _cache;

    public GetAllQueryHandler(DataDbContext context, IPaginationService pagination, IMemoryCache cache)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _pagination = pagination ?? throw new ArgumentNullException(nameof(pagination));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<PaginateResponse<List<Chat>>> Handle(GetAllQuery request, CancellationToken cancellationToken)
    {
        try
        {
            string cacheKey = "cacheAll";
            if (!_cache.TryGetValue(cacheKey, out List<Chat>? res))
            {
                res = await _context.Chats.AsNoTracking().ToListAsync();
                _cache.Set(cacheKey, res, TimeSpan.FromMinutes(10));
            }


            var queryData = res.AsEnumerable();
            if (!string.IsNullOrEmpty(request.SearchFilter))
            {
                queryData = queryData.Where(x => x.UserName.Contains(request.SearchFilter, StringComparison.OrdinalIgnoreCase) ||
                    x.ToUser.Contains(request.SearchFilter, StringComparison.OrdinalIgnoreCase) ||
                    x.Message.Contains(request.SearchFilter, StringComparison.OrdinalIgnoreCase));
            }

            var paginatedRes = await _pagination.Paginate(queryData.ToList(), request.pgFilter);

            return paginatedRes;
        }
        catch (Exception ex)
        {
            throw new NotFoundException($"Failed Due to ${ex.Message}");
        }
    }

}

