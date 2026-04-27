using Mvc_CRUD.Models;

namespace Mvc_CRUD.Dto;

    public class PostsDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string LastName { get; set; }
        public string? UserImageUrl { get; set; }
        public string? ImageContentUrl { get; set; }
        public string? Content { get; set; }
        public int? TotalComments { get; set; } = 0;
        public int? Shares { get; set; }
        public string? PostBgColour { get; set; }
        public string PostScope { get; set; }
        public string currentUserName { get; set; }
        public string currentUserLastName { get; set; }
        public string? currentUserProfilePic { get; set; }
        public string? Errors { get; set; } 
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public ICollection<LikesDto>? PostLikes { get; set; }
    }

