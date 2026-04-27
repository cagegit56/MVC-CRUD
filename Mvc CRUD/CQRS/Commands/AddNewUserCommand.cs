using MediatR;

namespace Mvc_CRUD.CQRS.Commands;

public record AddNewUserCommand : IRequest<(bool success, string error)>;
