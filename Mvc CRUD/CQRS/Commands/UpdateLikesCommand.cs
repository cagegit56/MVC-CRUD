using MediatR;

namespace Mvc_CRUD.CQRS.Commands;

public record UpdateLikesCommand(int postId, string Username) : IRequest<bool>;
