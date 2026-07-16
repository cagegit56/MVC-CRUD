namespace Mvc_CRUD.Dto;

    public class FriendUserProfileDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string LastName { get; set; }
        public string UserId { get; set; }
        public string? Email { get; set; }
        public string? UserProfilePicUrl { get; set; }
        public string? Errors { get; set; }

    }

