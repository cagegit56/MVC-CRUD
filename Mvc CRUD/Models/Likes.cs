namespace Mvc_CRUD.Models;

    public class Likes
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Lastname { get; set; }
        public string? UserProfilePicUrl { get; set; }
        public int PostId { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }

