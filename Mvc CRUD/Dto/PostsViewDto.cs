using Mvc_CRUD.Models;

namespace Mvc_CRUD.Dto;
    public class PostsViewDto
    {
        public string currentUserName { get; set; }
        public string currentUserLastName { get; set; }
        public string? currentUserProfilePic { get; set; }
        public string? Errors { get; set; }
        public ICollection<PostsDto>? Posts { get; set; }
    }

