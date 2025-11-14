using MediatR;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;

namespace Mvc_CRUD.CQRS.Queries;

    public record GetAllQuery(PaginationFilter pgFilter) : IRequest<PaginateResponse<List<Chat>>>;

