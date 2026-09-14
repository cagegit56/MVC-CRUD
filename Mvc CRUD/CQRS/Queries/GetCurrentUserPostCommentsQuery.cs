using MediatR;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;

namespace Mvc_CRUD.CQRS.Queries;

    public record GetCurrentUserPostCommentsQuery(int postId, PaginationFilter pgFilter) : IRequest<PaginateResponse<List<Comments>>>;
