using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Dto;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Queries;

    internal sealed class GetExternalUserProfileQueryHandler : IRequestHandler<GetExternalUserProfileQuery, UserProfileDTO>
    {
        private readonly DataDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<GetExternalUserProfileQueryHandler> _logger;

        public GetExternalUserProfileQueryHandler(DataDbContext context, IMapper mapper, ILogger<GetExternalUserProfileQueryHandler> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
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
                return mappedRes;
            }catch(Exception ex) {
                 _logger.LogError($"Failed to return user profile due to {ex.Message}");
                 return new UserProfileDTO() { Error = "Failed to return user profile." };
            }            
        }      
    }

