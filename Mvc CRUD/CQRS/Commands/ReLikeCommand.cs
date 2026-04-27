using MediatR;

namespace Mvc_CRUD.CQRS.Commands;

public record ReLikeCommand(int postId, string Username) : IRequest<bool>;
