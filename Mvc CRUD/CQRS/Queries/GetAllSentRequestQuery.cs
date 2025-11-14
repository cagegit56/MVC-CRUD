using MediatR;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;

namespace Mvc_CRUD.CQRS.Queries;

    public record GetAllSentRequestQuery(PaginationFilter pgFilter, string userId) : IRequest<PaginateResponse<List<FriendRequest>>>;

