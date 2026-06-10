using FluentResults;
using MediatR;
using Mvc_CRUD.Dto;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;

namespace Mvc_CRUD.CQRS.Queries;

    public record FriendRequestQuery(PaginationFilter pgFilter) : IRequest<Result<PaginateResponse<List<UserProfile>>>>;

