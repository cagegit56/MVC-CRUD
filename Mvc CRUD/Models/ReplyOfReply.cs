namespace Mvc_CRUD.Models;

    public class ReplyOfReply
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string LastName { get; set; }
        public string? UserImageUrl { get; set; }
        public string? Message { get; set; }
        public string? ImageContentUrl { get; set; }
        public int ReplyId { get; set; }
        public DateTime SentOn { get; set; } = DateTime.UtcNow;
        public CommentsReply? CommentReplies { get; set; }
    }

