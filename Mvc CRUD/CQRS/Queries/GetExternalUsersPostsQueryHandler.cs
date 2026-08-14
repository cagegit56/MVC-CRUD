using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Dto;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Queries;

    internal sealed class GetExternalUsersPostsQueryHandler : IRequestHandler<GetExternalUsersPostsQuery, PaginateResponse<List<PostsDto>>>
    {
        private readonly DataDbContext _context;
        private readonly IPaginationService _pagination;
        private readonly ILogger<GetExternalUsersPostsQueryHandler> _logger;

        public GetExternalUsersPostsQueryHandler(DataDbContext context, IPaginationService pagination, ILogger<GetExternalUsersPostsQueryHandler> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _pagination = pagination ?? throw new ArgumentNullException(nameof(pagination));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));           
        }

        public async Task<PaginateResponse<List<PostsDto>>> Handle(GetExternalUsersPostsQuery request, CancellationToken cancellationToken) 
        {
            if (string.IsNullOrEmpty(request.userId)) 
                return new PaginateResponse<List<PostsDto>>() { Error = "UserId Cannot be null or empty." };

            try
            {
                var res = await _context.Post
                          .Where(x => x.UserId == request.userId && x.PostScope == "Public").AsNoTracking().ToListAsync();
                 if (request.pgFilter.PageSize >= 50) request.pgFilter.PageSize = 5;
                var paginatedRes = _pagination.PaginateAndMap<Posts, PostsDto>(res, request.pgFilter);
                return paginatedRes;
            }
            catch (Exception ex) 
            {
                _logger.LogError($"Failed to return external user's posts due to {ex.Message}.");
                return new PaginateResponse<List<PostsDto>>() { Error = "Failed to return external user's posts." };
            }
        }
    }

