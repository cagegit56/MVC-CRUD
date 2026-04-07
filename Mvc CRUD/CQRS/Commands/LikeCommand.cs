using MediatR;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Commands;

public record LikeCommand(Likes model) : IRequest<bool>;

