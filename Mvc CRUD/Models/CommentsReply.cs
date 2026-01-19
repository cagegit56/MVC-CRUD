using System.ComponentModel.Design;

namespace Mvc_CRUD.Models;

    public class CommentsReply
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string LastName { get; set; }
        public string? UserImageUrl { get; set; }
        public string? Message { get; set; }
        public string? ImageContentUrl { get; set; }
        public int CommentId { get; set; }
        public string CommentUserName { get; set; }
        public string CommentLastName { get; set; }
        public string? CommentMessage { get; set; }
        public string? CommentImageContentUrl { get; set; }
        public DateTime SentOn { get; set; } = DateTime.UtcNow;
        public Comments? Comment { get; set; }

    }

