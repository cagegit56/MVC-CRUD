using MediatR;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Commands;
    public record AddFriendCommand(Friends model) : IRequest<string>;
