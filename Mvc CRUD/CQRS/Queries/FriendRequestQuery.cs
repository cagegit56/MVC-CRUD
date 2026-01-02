using MediatR;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;

namespace Mvc_CRUD.CQRS.Queries;

    public record FriendRequestQuery(PaginationFilter pgFilter, string UserId, string Username) : IRequest<PaginateResponse<List<Chat_Users>>>;

