namespace Mvc_CRUD.Models;

    public class FriendRequest
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string ToUserId { get; set; }
        public string ToUserName { get; set; }
        public string UserName { get; set; }
        public string? ToUserEmail { get; set; } 
        public string Status { get; set; } = "Pending";
        public DateTime SentOn { get; set; } = DateTime.UtcNow;
        public bool isDeleted { get; set; } = false;
    }

