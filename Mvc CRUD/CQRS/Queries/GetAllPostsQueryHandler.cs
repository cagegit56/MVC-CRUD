using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.CQRS.Commands;
using Mvc_CRUD.Dto;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Queries;

internal sealed class GetAllPostsQueryHandler : IRequestHandler<GetAllPostsQuery, List<PostsDto>>
{
    private readonly DataDbContext _context;
    private readonly IMediator _mediator;
    public GetAllPostsQueryHandler(DataDbContext context, IMediator mediator)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(_mediator));
    }
    public async Task<List<PostsDto>> Handle(GetAllPostsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var newUser = await _mediator.Send(new AddNewUserCommand());
            if (!newUser.success)
                throw new Exception($"Unable to add a user due to {newUser.error}");
            var friends = await _context.Friends.Where(x => x.UserName == request.CurrentUsername || x.FriendName == request.CurrentUsername)
                                 .Select(x => x.UserName).Distinct().ToListAsync();
            var res = await _context.Post.Where(x => x.PostScope == "Public" || (x.PostScope == "Friends" && friends.Contains(request.CurrentUsername))
                        || (x.PostScope == "Only me" && x.UserName == request.CurrentUsername))
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
                            PostScope = x.PostScope,
                            PostLikes = x.PostLikes.Where(g => g.IsDeleted != true).Select(k =>  new LikesDto()
                            {
                                Id = k.Id,
                                Username = k.Username,
                                Lastname = k.Lastname,
                                UserProfilePicUrl = k.UserProfilePicUrl,
                                PostId = k.PostId,
                                CreatedOn = k.CreatedOn,
                                IsDeleted = k.IsDeleted
                            }).ToList()
                        })
                        .OrderByDescending(x => x.CreatedOn).AsNoTracking().ToListAsync();
             return res;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to retrieve posts due to : {ex.Message}");
        }
    }
}

