using MediatR;
using Mvc_CRUD.Dto;
using Mvc_CRUD.Pagination;

namespace Mvc_CRUD.CQRS.Queries;

public record GetExternalUsersPostsQuery(string userId, PaginationFilter pgFilter) : IRequest<PaginateResponse<List<PostsDto>>>;

