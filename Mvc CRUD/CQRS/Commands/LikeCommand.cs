using MediatR;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Commands;

public record LikeCommand(int postId) : IRequest<(bool success, string error)>;

