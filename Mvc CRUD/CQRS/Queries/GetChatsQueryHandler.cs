using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Mvc_CRUD.Common;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;

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
                var friendInfo = await _context.Friends.Include(x => x.AllUsers).Where(x => x.UserId == response.UserId).ToListAsync();
                var friendName = friendInfo.Select(x => x.AllUsers?.UserName).ToList();
                var msg = await _context.Chats.Where(x => (x.UserName == response.UserName && friendName.Contains(x.ToUser)) || (friendName.Contains(x.UserName) && x.ToUser == response.UserName))
                                                .OrderByDescending(x => x.SentOn)
                                                .ToListAsync();
                var newFrnd = friendName.Where(f => !msg.Any(m => (m.UserName == response.UserName && m.ToUser == f) || (m.UserName == f && m.ToUser == response.UserName)))
                                        .Select(f => f)
                                        .ToList();
                var res = new List<Chat>();
                var newMessage = friendInfo.Where(x => x.UserName == response.UserName && newFrnd.Contains(x.FriendName)).ToList();
                if (newMessage.Count() != 0)
                {
                    foreach (var user in newMessage)
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
                res.AddRange(msg);

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
                throw new NotFoundException($"Failed Due to {ex.Message}");
            }
        }
       

    }

