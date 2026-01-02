namespace Mvc_CRUD.Models;

    public class Comments
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string? UserImageUrl { get; set; }
        public int PostId { get; set; }
        public string Message { get; set; }
        public DateTime SentOn { get; set; } = DateTime.UtcNow;
    }

