using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.CQRS.Queries;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class SendFriendRequestCommandHandler : IRequestHandler<SendFriendRequestCommand, Result>
{ 
    private readonly DataDbContext _context;
    private readonly IMediator _mediator;
    private readonly ILogger<SendFriendRequestCommandHandler> _logger;

    public SendFriendRequestCommandHandler(DataDbContext context, IMediator mediator, ILogger<SendFriendRequestCommandHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger;
    }

    public async Task<Result> Handle(SendFriendRequestCommand command, CancellationToken cancellationToken)
    {

        if (string.IsNullOrEmpty(command.model.ToUserId))
        {
            _logger.LogError("To userId cannot be null/empty.");
            return Result.Fail("To userId cannot be null/empty.");
        }

        try
        {
            var currentUser = await _mediator.Send(new GetUserProfileQuery());
            var exists = await _context.FriendRequests
                .FirstOrDefaultAsync(x => x.UserId == currentUser.UserId && x.ToUserId == command.model.ToUserId);
            if (exists is not null)
            {
                if (exists.Status == "Cancelled")
                {
                    exists.Status = "Pending";
                    _context.Update(exists);
                    await _context.SaveChangesAsync(cancellationToken);
                }
                return Result.Ok();
            }
            else
            {
                command.model.UserId = currentUser.UserId;
                command.model.UserName = currentUser.UserName;
                command.model.LastName = currentUser.LastName;
                command.model.ProfilePicUrl = currentUser.UserProfilePicUrl;
                await _context.AddAsync(command.model);
                await _context.SaveChangesAsync(cancellationToken);
                return Result.Ok();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to send a friend request from {command.model.UserName} to {command.model.ToUserName} to due to : {ex.Message}");
            return Result.Fail("Failed to send a friend request due to a technical issue.");
        }
    }
}