using FluentResults;
using MediatR;
using Mvc_CRUD.Common;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Commands;

    public record BlockUserCommand(BlockedUsers model) : IRequest<Result<string>>;

