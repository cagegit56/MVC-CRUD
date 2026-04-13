using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Dto;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Queries;

internal sealed class GetAllPostsQueryHandler : IRequestHandler<GetAllPostsQuery, List<PostsDto>>
{
    private readonly DataDbContext _context;
    public GetAllPostsQueryHandler(DataDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    public async Task<List<PostsDto>> Handle(GetAllPostsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var allowedScope = new[] { "Public", "Friends", "Only me" };
            var res = await _context.Post.Where(p => allowedScope.Contains(p.PostScope))
                .Select(x => new PostsDto()
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    LastName = x.LastName,
                    UserImageUrl = x.UserImageUrl,
                    ImageContentUrl = x.ImageContentUrl,
                    Content = x.Content,
                    PostBgColour = x.PostBgColour,
                    CreatedOn = x.CreatedOn,
                    TotalComments = x.TotalComments,
                    Shares = x.Shares,
                    PostLikes = x.PostLikes.Select(k =>  new LikesDto()
                    {
                        Id = k.Id,
                        Username = k.Username,
                        Lastname = k.Lastname,
                        UserProfilePicUrl = k.UserProfilePicUrl,
                        PostId = k.PostId,
                        CreatedOn = k.CreatedOn
                    }).ToList()
                })
                .OrderByDescending(x => x.CreatedOn).AsNoTracking().ToListAsync(cancellationToken);
            return res;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to retrieve posts due to : {ex.Message}");
        }
    }
}

