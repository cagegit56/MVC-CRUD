using MediatR;

namespace Mvc_CRUD.CQRS.Commands;

public record GalleryCommand(IFormFile image) : IRequest<bool>;

