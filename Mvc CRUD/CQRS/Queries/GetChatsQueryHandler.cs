using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Mvc_CRUD.Dto;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Queries;

internal sealed class GetChatsQueryHandler : IRequestHandler<GetChatsQuery, PaginateResponse<List<ChatsDto>>>
{
    private readonly DataDbContext _context;
    private readonly IPaginationService _pagination;
    private ICurrentUserProfile _currentUser;
    private ILogger<GetChatsQueryHandler> _logger;

    public GetChatsQueryHandler(DataDbContext context, IPaginationService pagination, ICurrentUserProfile currentUser,
         ILogger<GetChatsQueryHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _pagination = pagination ?? throw new ArgumentNullException(nameof(pagination));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(_currentUser));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    public async Task<PaginateResponse<List<ChatsDto>>> Handle(GetChatsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var chats = await _context.Chats.AsNoTracking()
               .Where(x => x.UserId == _currentUser.UserId || x.ToUserId == _currentUser.UserId).ToListAsync();

            var existingChatsId = new List<string?>();

            var chatsByCurrentUserId = chats.Where(x => x.UserId == _currentUser.UserId).Select(v => v.ToUserId).ToList();
            var chatsFromFriendsId = chats.Where(x => x.ToUserId == _currentUser.UserId).Select(v => v.UserId).ToList();

            existingChatsId.AddRange(chatsByCurrentUserId);
            existingChatsId.AddRange(chatsFromFriendsId);

            var newChat = await _context.Friends.AsNoTracking()
                .Where(x => (x.UserId == _currentUser.UserId && !existingChatsId.Contains(x.FriendId))
                || (x.FriendId == _currentUser.UserId && !existingChatsId.Contains(x.UserId)) && !x.IsDeleted)
                .Select(g => new Chat()
                {
                    UserName = g.UserName,
                    UserId = g.UserId,
                    LastName = g.LastName,
                    ProfilePicUrl = g.ProfilePicUrl,
                    Message = $"You are now Friends with {char.ToUpper(g.FriendName[0]) + g.FriendName.Substring(1)} Send a Message to Start a Chat.",
                    ToUserName = g.FriendName,
                    ToLastName = g.FriendLastName,
                    ToUserId = g.FriendId,
                    ToUserProfilePicUrl = g.FriendProfilePicUrl,
                    SentOn = g.CreatedOn
                }).ToListAsync();

            chats.AddRange(newChat);

            var deletedChats = chats.Where(x => x.IsDeleted).ToList();
            if (deletedChats.Any())
            {
                chats = chats.Where(x => !x.IsDeleted).ToList();
                foreach (var msg in deletedChats.DistinctBy(x => x.UserName))
                {
                    msg.Message = "";
                    msg.SentOn = new DateTime(2020, 02, 20, 20, 20, 20);
                    chats.Add(msg);
                }  
            }

            var res = chats.OrderByDescending(s => s.SentOn).Select(x => new ChatsDto() {
                Id = x.Id,
                UserName = x.UserName == _currentUser.UserName ? x.ToUserName : x.UserName,
                LastName = x.LastName == _currentUser.LastName ? x.ToLastName! : x.LastName,
                UserId = x.UserId == _currentUser.UserId ? x.ToUserId : x.UserId,
                ProfilePicUrl = x.ProfilePicUrl == _currentUser.ProfilePicUrl ? x.ToUserProfilePicUrl : x.ProfilePicUrl,
                Message = x.Message,
                ToUserName = x.ToUserName != _currentUser.UserName ? x.UserName : x.ToUserName,
                ToLastName = x.ToLastName != _currentUser.LastName ? x.LastName : x.ToLastName,
                ToUserProfilePicUrl = x.ToUserProfilePicUrl != _currentUser.ProfilePicUrl ? x.ProfilePicUrl : x.ToUserProfilePicUrl,
                SentOn = x.SentOn,
                Sender = x.UserName == _currentUser.UserName ? "CurrentUser" : "Receiver",
            }).DistinctBy(g => g.UserName).ToList();

            var paginatedResults = await _pagination.Paginate(res, request.pgFilter, cancellationToken);
            paginatedResults.UserName = _currentUser.UserName;
            paginatedResults.ProfilePicUrl = _currentUser.ProfilePicUrl;

            return paginatedResults;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to return chats Due to : {ex.Message}");
            return new PaginateResponse<List<ChatsDto>>
            {
                Error = "Failed to return chats Due to a technical issue.",
                UserName = _currentUser.UserName,
                ProfilePicUrl = _currentUser.ProfilePicUrl,
            };
        }
    }
}

