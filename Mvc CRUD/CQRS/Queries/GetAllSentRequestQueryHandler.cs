using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Dto;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Queries;

    internal sealed class GetAllSentRequestQueryHandler :IRequestHandler<GetAllSentRequestQuery, Result<PaginateResponse<List<FriendRequestDto>>>>
    {
        private readonly DataDbContext _context;
        private readonly IUserInfoContextService _currentUser;
        private readonly IPaginationService _pagination;
        private readonly ILogger<GetAllSentRequestQueryHandler> _logger;
        public GetAllSentRequestQueryHandler(DataDbContext context, IUserInfoContextService currentUser, IPaginationService pagination, ILogger<GetAllSentRequestQueryHandler> logger)
        {
           _context = context ?? throw new ArgumentNullException(nameof(context));
           _currentUser = currentUser ?? throw new ArgumentNullException(nameof(_currentUser));
           _pagination = pagination ?? throw new ArgumentNullException(nameof(pagination));
           _logger = logger;
          
        }
        public async Task<Result<PaginateResponse<List<FriendRequestDto>>>> Handle(GetAllSentRequestQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var res = await _context.FriendRequests
                                 .Where(x => x.UserId == _currentUser.UserId && x.Status == "Pending" && x.isDeleted != true)
                                 .GroupBy(x => x.ToUserId).Select(k => k.First()).AsNoTracking().ToListAsync();
                return Result.Ok( _pagination.PaginateAndMap<FriendRequest, FriendRequestDto>(res, request.pgFilter));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get all sent friend request data due to : {ex.Message}");               
                return Result.Ok(new PaginateResponse<List<FriendRequestDto>>()
                {
                    Error = "Failed to get all sent friend request check inner exception for more details."
                });
            }
        }
    }

