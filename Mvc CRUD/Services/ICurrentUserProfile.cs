using Mvc_CRUD.Dto;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.Services;

    public interface ICurrentUserProfile
    {
        UserProfileDTO? UserProfileObj { get; }
        string? UserName { get; }
        string? UserId { get; }
        string? LastName { get; }
        string? ProfilePicUrl { get; }
        string? Email { get; }
    }

