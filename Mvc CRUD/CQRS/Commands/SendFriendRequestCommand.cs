using MediatR;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Commands;
    public record SendFriendRequestCommand(FriendRequest model) : IRequest<string>;
