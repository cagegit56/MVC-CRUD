using MediatR;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Commands;

public record CreatePostCommand(Posts model, IFormFile postImage) : IRequest<bool>;
