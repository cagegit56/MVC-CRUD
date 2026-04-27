using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.CQRS.Commands;
using Mvc_CRUD.Dto;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Queries;

internal sealed class GetAllPostsQueryHandler : IRequestHandler<GetAllPostsQuery, PostsViewDto>
{
    private readonly DataDbContext _context;
    private readonly IMediator _mediator;
    public GetAllPostsQueryHandler(DataDbContext context, IMediator mediator)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(_mediator));
    }
    public async Task<PostsViewDto> Handle(GetAllPostsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var newUser = await _mediator.Send(new AddNewUserCommand());
            if (!newUser.success)
                throw new Exception($"Unable to add a user due to {newUser.error}");

            var CurrentUser = await _mediator.Send(new GetUserProfileQuery());

            var friends = _context.Friends.Where(x => x.UserName == CurrentUser.UserName || x.FriendName == CurrentUser.UserName)
                          .Select(f => f.UserName == CurrentUser.UserName ? f.FriendName : f.UserName).Distinct();

            var results = await _context.Post.Where(x => x.PostScope == "Public"
                    || (x.PostScope == "Friends" && (x.UserName == CurrentUser.UserName || friends.Contains(x.UserName))
                    || (x.PostScope == "Only me" && x.UserName == CurrentUser.UserName)))
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
                        PostLikes = x.PostLikes.Where(g => g.IsDeleted != true).Select(k => new LikesDto()
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
                    .OrderByDescending(x => x.CreatedOn).AsNoTracking().ToListAsync(cancellationToken);
            var res = new PostsViewDto
            {
                Posts = results,
                currentUserName = CurrentUser.UserName!,
                currentUserLastName = CurrentUser.LastName!,
                currentUserProfilePic = CurrentUser.UserProfilePicUrl,
            };      
            return res;
        }
        catch (Exception ex)
        {
            var error = new PostsViewDto();
            error.Errors = $"Failed to retrieve posts due to : {ex.Message}";
            return error;
        }
    }
}


 //|| _context.Friends.Any(f => (f.UserName == x.UserName && f.FriendName == _currentUserInfo.UserName)))


//***********************it works but (it does not return all the post with a public scope) *********************************************************
//******************** another reason is that we don't need any data from friends table we just checking if a friend exist or not,
//*****************so this is not totally a good idea, it would work provided we need data from friends table. ************************************** 

//var res = await (from p in _context.Post
//                 join f in _context.Friends on
//                 p.UserName equals f.UserName
//                 where p.PostScope == "Public" ||
//                 (p.PostScope == "Friends" && (p.UserName == request.CurrentUsername || (f.UserName == request.CurrentUsername && f.FriendName == p.UserName))
//                 || (f.UserName == p.UserName && f.FriendName == request.CurrentUsername))
//                 || (p.PostScope == "Only me" && p.UserName == request.CurrentUsername)
//                 select new PostsDto()
//                 {
//                     Id = p.Id,
//                     UserName = p.UserName,
//                     LastName = p.LastName,
//                     UserImageUrl = p.UserImageUrl,
//                     ImageContentUrl = p.ImageContentUrl,
//                     Content = p.Content,
//                     PostBgColour = p.PostBgColour,
//                     CreatedOn = p.CreatedOn,
//                     TotalComments = p.TotalComments,
//                     Shares = p.Shares,
//                     PostScope = p.PostScope,
//                     PostLikes = p.PostLikes.Where(g => g.IsDeleted != true).Select(k => new LikesDto()
//                     {
//                         Id = k.Id,
//                         Username = k.Username,
//                         Lastname = k.Lastname,
//                         UserProfilePicUrl = k.UserProfilePicUrl,
//                         PostId = k.PostId,
//                         CreatedOn = k.CreatedOn,
//                         IsDeleted = k.IsDeleted
//                     }).ToList(),
//                 }).OrderByDescending(r => r.CreatedOn).AsNoTracking().ToListAsync();




