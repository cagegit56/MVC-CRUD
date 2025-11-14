using MediatR;

namespace Mvc_CRUD.CQRS.Commands;

    public record RejectRequestCommand(string userId, string toUserId) : IRequest<string>;

