using MediatR;

namespace Mvc_CRUD.CQRS.Commands;

    public record RejectRequestCommand(string toUserId) : IRequest<bool>;

