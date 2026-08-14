using System.ComponentModel.DataAnnotations.Schema;

namespace Mvc_CRUD.Models;

    public class Chat
    {
       public int Id { get; set; }
       public string UserName { get; set; }
       public string? LastName { get; set; }
       public string UserId { get; set; }
       public string? ProfilePicUrl { get; set; }
       public string ToUserName { get; set; } 
       public string? ToLastName { get; set; }
       public string ToUserId { get; set; }
       public string? ToUserProfilePicUrl { get; set; }
       public string Message { get; set; }
       public string Status { get; set; } = "New";
       public bool IsDeleted { get; set; } = false;       
       public DateTime SentOn { get; set; } = DateTime.UtcNow;       
    }

