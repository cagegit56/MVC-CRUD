using Mvc_CRUD.Models;

namespace Mvc_CRUD.Dto;

    public class CommentsDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string LastName { get; set; }
        public string UserId { get; set; }
        public string? UserImageUrl { get; set; }
        public int PostId { get; set; }
        public string Message { get; set; }
        public DateTime SentOn { get; set; }
        public ICollection<CommentsReplyDto>? Reply { get; set; }
    }

