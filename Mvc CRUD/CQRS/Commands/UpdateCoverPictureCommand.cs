using MediatR;

namespace Mvc_CRUD.CQRS.Commands;

    public record UpdateCoverPictureCommand(IFormFile image) : IRequest<bool>;
