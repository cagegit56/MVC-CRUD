using MediatR;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;

namespace Mvc_CRUD.CQRS.Queries;

    public record ReceivedFriendRequestQuery(PaginationFilter pgFilter) : IRequest<PaginateResponse<List<FriendRequest>>>;

