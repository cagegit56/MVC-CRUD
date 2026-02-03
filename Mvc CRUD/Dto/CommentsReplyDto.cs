using Mvc_CRUD.Models;

namespace Mvc_CRUD.Dto;

    public class CommentsReplyDto
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? LastName { get; set; }
        public string? UserImageUrl { get; set; }
        public string? Message { get; set; }
        public string? ImageContentUrl { get; set; }
        public int CommentId { get; set; }
        public DateTime SentOn { get; set; } = DateTime.UtcNow;
        public ICollection<ReplyOfReplyDto>? Replies { get; set; }
    }

