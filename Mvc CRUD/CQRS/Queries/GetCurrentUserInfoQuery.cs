using MediatR;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Queries;

    public record GetCurrentUserInfoQuery(string UserId) : IRequest<Chat_Users>;

