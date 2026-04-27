using MediatR;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Queries;

    public record GetUserProfileQuery : IRequest<UserProfile>;