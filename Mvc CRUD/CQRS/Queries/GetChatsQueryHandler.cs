using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Mvc_CRUD.Common;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;
using System.Linq;

namespace Mvc_CRUD.CQRS.Queries;

    internal sealed class GetChatsQueryHandler : IRequestHandler<GetChatsQuery, List<Chat>>
    {
       private readonly DataDbContext _context;
       private IMemoryCache _cache; 
       public GetChatsQueryHandler(DataDbContext context, IMemoryCache cache)
        {
           _context = context ?? throw new ArgumentNullException(nameof(context));
           _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }
       public async Task<List<Chat>> Handle(GetChatsQuery response, CancellationToken cancellationToken)
        {
            try
            {
                var chats = await _context.Chats.Where(x => _context.Friends
                                .Any(f => f.UserId == response.UserId &&
                                ( (x.UserName == response.UserName && x.ToUser == f.FriendName) ||
                                (x.ToUser == response.UserName && x.UserName == f.FriendName)) )).ToListAsync();
                var chatFrnd = chats.Select(x => x.UserName == response.UserName ? x.ToUser : x.UserName).ToList();
                var noChat = await _context.Friends.Where(x => x.UserId == response.UserId &&
                                  !chatFrnd.Contains(x.FriendName)).ToListAsync();
                var res = new List<Chat>();
                    if (noChat.Count() != 0)
                    {
                        foreach (var user in noChat)
                        {
                            var emptyMsg = new Chat()
                            {
                                UserName = response.UserName,
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
                        res = res.Where(x => (x.UserName.Equals(response.UserName, StringComparison.OrdinalIgnoreCase) && x.ToUser.Equals(response.ToFriend, StringComparison.OrdinalIgnoreCase))
                        || (x.UserName.Equals(response.ToFriend, StringComparison.OrdinalIgnoreCase) && x.ToUser.Equals(response.UserName, StringComparison.OrdinalIgnoreCase)))
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

