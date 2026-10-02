using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Dto;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Queries;

    internal sealed class GetExternalUserProfileQueryHandler : IRequestHandler<GetExternalUserProfileQuery, UserProfileDTO>
    {
        private readonly DataDbContext _context;
        private readonly ICurrentUserProfile _currentUser;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<GetExternalUserProfileQueryHandler> _logger;

        public GetExternalUserProfileQueryHandler(DataDbContext context, ICurrentUserProfile currentUser, IMediator mediator, IMapper mapper,
               ILogger<GetExternalUserProfileQueryHandler> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<UserProfileDTO> Handle(GetExternalUserProfileQuery request, CancellationToken cancellationToken) 
        {
            if (string.IsNullOrEmpty(request.userId)) 
                return new UserProfileDTO() { Error = "UserId cannot be null or empty." };
            try
            {
                var res =  _context.Profile.AsNoTracking().FirstOrDefault(x => x.UserId == request.userId);
                var mappedRes =  _mapper.Map<UserProfileDTO>(res);
                var friendShipStatus = await _context.Friends
                                        .AnyAsync(x => (x.UserId == _currentUser.UserId && x.FriendId == request.userId)
                                        || (x.UserId == request.userId && x.FriendId == _currentUser.UserId));
                if (friendShipStatus)
                {
                    mappedRes.FriendStatus = true;
                    return mappedRes;
                }
                var friendRequestStatus = await _context.FriendRequests
                                           .Where(x => ( (x.UserId == _currentUser.UserId && x.ToUserId == request.userId) 
                                           || (x.UserId == request.userId && x.ToUserId == _currentUser.UserId) )
                                           && x.Status == "Pending" && !x.isDeleted).FirstOrDefaultAsync();
                if (friendRequestStatus != null) 
                {
                    mappedRes.PendingRequest = true;
                    if (friendRequestStatus.UserId == _currentUser.UserId)
                        mappedRes.RequestSentByMe = true;
                }

                mappedRes.CurrentUserId = _currentUser.UserId;
                mappedRes.CurrentUserName = _currentUser.UserName;
                mappedRes.CurrentUserProfilePicUrl = _currentUser.ProfilePicUrl;

                return mappedRes;
            }catch(Exception ex) {
                 _logger.LogError($"Failed to return user profile due to {ex.Message}");
                 return new UserProfileDTO() { Error = "Failed to return user profile." };
            }            
        }      
    }

