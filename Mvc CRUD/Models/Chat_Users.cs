namespace Mvc_CRUD.Models;

    public class Chat_Users
    {
    public int Id { get; set; }
    public string Email { get; set; }
    public string UserId { get; set; }
    public string? UserName {get; set;}
    public string FirstName { get; set; } 
    public string? LastName { get; set; } 
    public string? ProfilePicPath { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    }

