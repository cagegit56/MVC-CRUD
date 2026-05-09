using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Mvc_CRUD.Common;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;
using Mvc_CRUD.Services;
using System.Linq;

namespace Mvc_CRUD.CQRS.Queries;

    internal sealed class GetChatsQueryHandler : IRequestHandler<GetChatsQuery, List<Chat>>
    {
       private readonly DataDbContext _context;
       private IUserInfoContextService _currentUser;
       public GetChatsQueryHandler(DataDbContext context, IUserInfoContextService currentUser)
        {
           _context = context ?? throw new ArgumentNullException(nameof(context));
           _currentUser = currentUser ?? throw new ArgumentNullException(nameof(_currentUser));
        }
       public async Task<List<Chat>> Handle(GetChatsQuery response, CancellationToken cancellationToken)
        {
            try
            {
                var chats = await _context.Chats.Where(x => _context.Friends
                                .Any(f => f.UserId == _currentUser.UserId &&
                                ( (x.UserName == _currentUser.UserName && x.ToUser == f.FriendName) ||
                                (x.ToUser == _currentUser.UserName && x.UserName == f.FriendName)) )).ToListAsync();
                var chatFrnd = chats.Select(x => x.UserName == _currentUser.UserName ? x.ToUser : x.UserName).ToList();
                var noChat = await _context.Friends.Where(x => x.UserId == _currentUser.UserId &&
                                  !chatFrnd.Contains(x.FriendName)).ToListAsync();
                var res = new List<Chat>();
                    if (noChat.Count() != 0)
                    {
                        foreach (var user in noChat)
                        {
                            var emptyMsg = new Chat()
                            {
                                UserName = _currentUser.UserName!,
                                ToUser = user.FriendName,
                                Message = $"You are now Friends with {char.ToUpper(user.FriendName[0]) + user.FriendName.Substring(1)} Send a Message to Start a Chat.",
                                SentOn = user.CreatedOn,
                            };
                            res.Add(emptyMsg);
                        }

                    }
                    res.AddRange(chats);

                    if (!string.IsNullOrEmpty(response.ToFriend))
                    {
                        res = res.Where(x => (x.UserName.Equals(_currentUser.UserName, StringComparison.OrdinalIgnoreCase) && x.ToUser.Equals(response.ToFriend, StringComparison.OrdinalIgnoreCase))
                        || (x.UserName.Equals(response.ToFriend, StringComparison.OrdinalIgnoreCase) && x.ToUser.Equals(_currentUser.UserName, StringComparison.OrdinalIgnoreCase)))
                            .OrderBy(x => x.SentOn)
                            .ToList();
                        return res;
                    }

                    return res;

            }catch(Exception ex)
            {
                throw new Exception($"Failed Due to {ex.Message}");
            }
        }
       

    }

