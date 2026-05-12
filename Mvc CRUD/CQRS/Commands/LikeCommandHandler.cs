using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.CQRS.Queries;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class LikeCommandHandler : IRequestHandler<LikeCommand, bool>
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

    public async Task<bool> Handle(LikeCommand request, CancellationToken cancellationToken)
    {
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
                return false;
            }
            
            model.UserProfilePicUrl = currentUser?.UserProfilePicUrl;
            model.PostId = request.postId;
            var checkExistence = await _context.Like.Where(x => x.PostId == request.postId &&
                                  x.Username == currentUser!.UserName).FirstOrDefaultAsync();
            if (checkExistence != null)
            {
                return await _mediator.Send(new ReLikeCommand(checkExistence.PostId, checkExistence.Username));
            }
            else
            {
                await _context.AddAsync(model, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }           
        }
        catch (Exception ex) {
            _logger.LogError($"Failed to add a like due to : {ex.Message}");
            return false;
        }
    }
}

