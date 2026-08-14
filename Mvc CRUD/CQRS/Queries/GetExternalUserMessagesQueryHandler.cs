using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Dto;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Queries;

internal sealed class GetExternalUserMessagesQueryHandler : IRequestHandler<GetExternalUserMessagesQuery, PaginateResponse<List<ExternalUserMessagesDto>>>
{
    private readonly DataDbContext _context;
    private readonly IPaginationService _pagination;
    private readonly IUserInfoContextService _currentUser;
    private readonly ILogger<GetExternalUserMessagesQueryHandler> _logger;

    public GetExternalUserMessagesQueryHandler(DataDbContext context, IPaginationService pagination, IUserInfoContextService currentUser,
       ILogger<GetExternalUserMessagesQueryHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _pagination = pagination ?? throw new ArgumentNullException(nameof(pagination));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));        
    }

    public async Task<PaginateResponse<List<ExternalUserMessagesDto>>> Handle(GetExternalUserMessagesQuery request, CancellationToken cancellationToken)
    {
        if(string.IsNullOrEmpty(request.toUserId))
            return new PaginateResponse<List<ExternalUserMessagesDto>> { Error = "ToUserId cannot be null" };
        try
        {
            var res = await _context.Chats.Where(x => (x.UserId == _currentUser.UserId && x.ToUserId == request.toUserId)
                           || (x.UserId == request.toUserId && x.ToUserId == _currentUser.UserId))
                           .AsNoTracking().ToListAsync(cancellationToken);
            if (request.pgFilter.PageSize >= 50) request.pgFilter.PageSize = 5;
            request.pgFilter.SortBy = "asc";
            var paginatedRes = _pagination.PaginateAndMap<Chat, ExternalUserMessagesDto>(res, request.pgFilter);
            return paginatedRes;
        }
        catch (Exception ex) {
            _logger.LogError($"Failed to get external user messages due to {ex.Message}.");
            return new PaginateResponse<List<ExternalUserMessagesDto>> { Error = "Failed to get external user messages." };
        }        
    }
}

