using MediatR;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;

namespace Mvc_CRUD.CQRS.Queries;

    public record GetChatsQuery(string UserId, string UserName, string ToFriend) : IRequest<List<Chat>>;

