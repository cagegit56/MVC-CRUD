namespace Mvc_CRUD.Dto;

    public class FriendRequestDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string LastName { get; set; }
        public string? ProfilePicUrl { get; set; }
        public string ToUserId { get; set; }
        public string ToUserName { get; set; }
        public string ToUser_LastName { get; set; }
        public string? ToUser_ProfilePicUrl { get; set; }
        public DateTime SentOn { get; set; } = DateTime.UtcNow;
    }

