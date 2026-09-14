using FluentResults;
using MediatR;

namespace Mvc_CRUD.CQRS.Commands;

    public record RemoveProfilePictureCommand : IRequest<Result>;

