using FluentResults;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Mvc_CRUD.CQRS.Queries;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Commands;

    internal sealed class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, Result<string>>
    {
        private readonly DataDbContext _context;
        private readonly IMediator _mediator;
        private readonly ILogger<SendMessageCommandHandler> _logger;
        private readonly IMemoryCache _cache;

        public SendMessageCommandHandler(DataDbContext context, IMediator mediator,
            ILogger<SendMessageCommandHandler> logger, IMemoryCache cache)
        {
           _context = context ?? throw new ArgumentNullException(nameof(context));
           _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
           _logger = logger;
           _cache = cache;
        }
        public async Task<Result<string>> Handle(SendMessageCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(command.model.ToUserName) || string.IsNullOrEmpty(command.model.ToUserId))
            {
                _logger.LogError("ToUsername or ToUserId is null.");
                return Result.Fail("To username or to userid cannot be null.");
            }
                
            if (string.IsNullOrEmpty(command.model.Message))
            {
                _logger.LogError("Message is empty.");
                return Result.Fail("Cannot send an empty message.");
            }               

            try
            {
                var currentUser = await _mediator.Send(new GetUserProfileQuery());
                command.model.UserName = currentUser.UserName;
                command.model.UserId = currentUser.UserId;
                command.model.LastName= currentUser.LastName;
                command.model.ProfilePicUrl = currentUser.UserProfilePicUrl;

                var res = await _context.Chats.AddAsync(command.model);
                await _context.SaveChangesAsync(cancellationToken);
                _cache.Remove("cacheAll");

                return Result.Ok("Message succesfully sent");
            }
            catch (Exception ex) 
            {
                _logger.LogError($"Failed to send message due to : {ex.Message}");
                return Result.Fail("Failed to send message");
            }           
        }
    }

