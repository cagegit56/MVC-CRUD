using MediatR;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Commands;

public record CreatePostCommand(string content, string selectedColor, IFormFile postImage, string Scope) 
                                : IRequest<bool>;
