using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Dto;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Queries;

internal sealed class GetAllChatsQueryHandler : IRequestHandler<GetAllChatsQuery, PaginateResponse<List<ChatsDto>>>
{
    private readonly DataDbContext _context;
    private readonly IUserInfoContextService _currentUser;
    private readonly IPaginationService _pagination;    
    private readonly ILogger<GetAllChatsQueryHandler> _logger;

    public GetAllChatsQueryHandler(DataDbContext context, IUserInfoContextService currentUser, IPaginationService pagination,
        ILogger<GetAllChatsQueryHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _pagination = pagination ?? throw new ArgumentNullException(nameof(pagination));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PaginateResponse<List<ChatsDto>>> Handle(GetAllChatsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.userId)) return new PaginateResponse<List<ChatsDto>> { Error = "User Id cannot be null/empty." };

        try
        {
            var res = await _context.Chats.AsNoTracking()
                .Where(x => (x.UserId == _currentUser.UserId && x.ToUserId == request.userId) 
                || (x.UserId == request.userId && x.ToUserId == _currentUser.UserId) ).ToListAsync();

            if (res.Any())
            {
                var deletedChat = res.Where(x => x.IsDeleted).FirstOrDefault();
                if (deletedChat != null)
                {
                    res = res.Where(x => !x.IsDeleted).ToList();
                    res.Add(new Chat() { Message = "DeletedChat" });
                }
            }
            else {
                res.Add(new Chat() { Message = "NewChat" });
            }

            var paginatedRes = _pagination.PaginateAndMap<Chat, ChatsDto>(res, request.pgFilter, cancellationToken);

            return paginatedRes;
        }
        catch (Exception ex) {
            _logger.LogError($"Failed to return chats due to : {ex.Message}");
            return new PaginateResponse<List<ChatsDto>> { Error = "Failed to return chats due to a technical issue." };
        }
    }
}

