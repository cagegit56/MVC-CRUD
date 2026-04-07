using MediatR;

namespace Mvc_CRUD.CQRS.Commands;

public record UnlikePostCommand(int postId, string userName) : IRequest<bool>;
