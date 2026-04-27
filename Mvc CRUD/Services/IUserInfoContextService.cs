namespace Mvc_CRUD.Services;
    public interface IUserInfoContextService
    {
       string? UserName { get; }
       string? UserId { get; }
       string? FirstName { get; }
       string? LastName { get; }
       string? Email { get; }
    }
