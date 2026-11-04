using FluentResults;
using MediatR;

namespace Mvc_CRUD.CQRS.Commands;

    public record ConvertMessageToSeenCommand(string userId) : IRequest<Result>;
