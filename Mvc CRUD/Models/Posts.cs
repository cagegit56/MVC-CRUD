namespace Mvc_CRUD.Models;

    public class Posts
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string LastName { get; set; }
        public string? UserImageUrl { get; set; }
        public string? ImageContentUrl { get; set; }
        public string Content { get; set; }
        public int? likes { get; set; }
        public int? TotalComments { get; set; }
        public int? Shares { get; set; }
        public bool Isliked { get; set; }
        public string PostScope { get; set; }
        public string? PostBgColour { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public  ICollection<Comments>? Comments { get; set; }
    }

