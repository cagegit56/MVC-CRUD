using Mvc_CRUD.Models;

namespace Mvc_CRUD.Dto;

    public class ChatsDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string LastName { get; set; }
        public string UserId { get; set; }
        public string? ProfilePicUrl { get; set; }
        public string ToUserName { get; set; }
        public string? ToLastName { get; set; }
        public string ToUserId { get; set; }
        public string? ToUserProfilePicUrl { get; set; }
        public string Message { get; set; }
        public DateTime SentOn { get; set; }
        public string? ChatStatus { get; set; }
        public string? Sender { get; set; } 
    }

