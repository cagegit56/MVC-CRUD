using System.ComponentModel.DataAnnotations.Schema;

namespace Mvc_CRUD.Models;

    [NotMapped]
    public class Post
    {
        public int Id { get; set; }
        public string AuthorName { get; set; }
        public string AuthorAvatar { get; set; }
        public string Content { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime PostedAt { get; set; }
        public int Likes { get; set; }
        public int Shares { get; set; }
        public bool IsLiked { get; set; }
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }

    [NotMapped]
    public class Comment
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string UserAvatar { get; set; }
        public string Text { get; set; }
        public string TimeAgo { get; set; }
    }


// ********************my model
    //public int Id { get; set; }
    //public string UserId { get; set; }
    //public string UserName { get; set; }
    //public IFormFile UserImage { get; set; }
    //public IFormFile ImageUrl { get; set; }
    //public string Content { get; set; }
    //public string PostBgColour { get; set; }
    //public DateTime CreatedOn { get; set; } = DateTime.UtcNow;


