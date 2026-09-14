using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.CQRS.Queries;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class LikeCommandHandler : IRequestHandler<LikeCommand, (bool success, string error)>
{
    private readonly DataDbContext _context;
    private readonly IMediator _mediator;
    private readonly ILogger<LikeCommandHandler> _logger;
    public LikeCommandHandler(DataDbContext context, IMediator mediator, ILogger<LikeCommandHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<(bool success, string error)> Handle(LikeCommand request, CancellationToken cancellationToken)
    {
        if (request.postId == 0)
        {
            _logger.LogError("Post id is missing or null..");
            return (false, "Post id is missing or null..");
        }
            
        try
        {
            var model = new Likes();
            var currentUser = await _mediator.Send(new GetUserProfileQuery());
            if (currentUser.UserName != null && currentUser.LastName != null)
            {
                model.Username = currentUser.UserName;
                model.Lastname = currentUser.LastName;
            }
            else
            {
                _logger.LogError("Current user info cannot be null.");
                return (false, "Current user info missing.");
            }
            
            model.UserProfilePicUrl = currentUser?.UserProfilePicUrl;
            model.PostId = request.postId;
            var checkExistence = await _context.Like.Where(x => x.PostId == request.postId &&
                                  x.Username == currentUser!.UserName).FirstOrDefaultAsync();
            if (checkExistence != null)
            {
                var res = await _mediator.Send(new ReLikeCommand(checkExistence.PostId, checkExistence.Username));
                if (res.IsSuccess) return (true, "");
                return (false, "Failed to re-like");
            }
            else
            {
                await _context.AddAsync(model, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return (true, "");
            }           
        }
        catch (Exception ex) {
            _logger.LogError($"Failed to add a like due to : {ex.Message}");
            return (false, "Failed to like check logger message.");
        }
    }
}

