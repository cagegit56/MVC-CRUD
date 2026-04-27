using MediatR;
using Mvc_CRUD.Dto;

namespace Mvc_CRUD.CQRS.Queries;

public record GetCommentsQuery(int postId) : IRequest<List<CommentsDto>>;

