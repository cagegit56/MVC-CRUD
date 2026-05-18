using MediatR;

namespace Mvc_CRUD.CQRS.Commands;

public record UpdateProfilePictureCommand(IFormFile image) : IRequest<bool>;

