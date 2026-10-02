using System.ComponentModel.DataAnnotations;

namespace Mvc_CRUD.Models;

    public class Friends
    {
        public int Id { get; set; }
        public string UserId { get; set; } 
        public string UserName { get; set; }
        public string? ProfilePicUrl { get; set; }
        public string LastName { get; set; }
        public string FriendId { get; set; }
        public string FriendName { get; set; }
        public string FriendLastName { get; set; }
        public string? FriendProfilePicUrl { get; set; } 
        public bool IsDeleted { get; set; } = false;
        public string Status { get; set; } = "online";
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }

