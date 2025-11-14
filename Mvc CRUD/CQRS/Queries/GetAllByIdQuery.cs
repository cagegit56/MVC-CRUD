using MediatR;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;

namespace Mvc_CRUD.CQRS.Queries;

    public record GetAllByIdQuery(PaginationFilter pgFilter, int Id) : IRequest<Chat>;

