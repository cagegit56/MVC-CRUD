using FluentResults;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Mvc_CRUD.CQRS.Queries;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Commands;

    internal sealed class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, Result>
    {
        private readonly DataDbContext _context;
        private readonly ICurrentUserProfile _currentUser;
        private readonly ILogger<SendMessageCommandHandler> _logger;
        private readonly IMemoryCache _cache;

        public SendMessageCommandHandler(DataDbContext context, ICurrentUserProfile currentUser,
            ILogger<SendMessageCommandHandler> logger, IMemoryCache cache)
        {
           _context = context ?? throw new ArgumentNullException(nameof(context));
           _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
           _logger = logger;
           _cache = cache;
        }
        public async Task<Result> Handle(SendMessageCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(command.model.ToUserName) || string.IsNullOrEmpty(command.model.ToUserId) || string.IsNullOrEmpty(command.model.ToLastName))
            {
                _logger.LogError("Friend's name, userId or lastname cannot be null.");
                return Result.Fail("Friend's name, userId or lastname cannot be null.");
            }
                
            if (string.IsNullOrEmpty(command.model.Message))
            {
                _logger.LogError("Cannot send an empty message.");
                return Result.Fail("Cannot send an empty message.");
            }               

            try
            {
                command.model.UserName = _currentUser.UserName!;
                command.model.UserId = _currentUser.UserId!;
                command.model.LastName= _currentUser.LastName!;
                command.model.ProfilePicUrl = _currentUser.ProfilePicUrl;

                var res = await _context.Chats.AddAsync(command.model);
                await _context.SaveChangesAsync(cancellationToken);

                return Result.Ok();
            }
            catch (Exception ex) 
            {
                _logger.LogError($"Failed to send message due to : {ex.Message}");
                return Result.Fail("Failed to send message due to a technical issue.");
            }           
        }
    }

