using System.ComponentModel.Design;

namespace Mvc_CRUD.Models;

    public class CommentsReply
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string LastName { get; set; }
        public string? UserImageUrl { get; set; }
        public string Message { get; set; }
        public string? ImageContentUrl { get; set; }
        public int CommentId { get; set; }
        public DateTime SentOn { get; set; } = DateTime.UtcNow;
        public Comments? Comment { get; set; }
        public ICollection<ReplyOfReply>? Replies { get; set; }

    }

