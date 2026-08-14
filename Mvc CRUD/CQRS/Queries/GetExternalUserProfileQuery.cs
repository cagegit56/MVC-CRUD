using MediatR;
using Mvc_CRUD.Dto;
using Mvc_CRUD.Pagination;

namespace Mvc_CRUD.CQRS.Queries;

public record GetExternalUserProfileQuery(string userId, PaginationFilter pgFilter) : IRequest<UserProfileDTO>;

