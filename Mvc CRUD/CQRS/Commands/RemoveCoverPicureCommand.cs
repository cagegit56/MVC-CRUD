using FluentResults;
using MediatR;

namespace Mvc_CRUD.CQRS.Commands;

    public record RemoveCoverPicureCommand : IRequest<Result>;
