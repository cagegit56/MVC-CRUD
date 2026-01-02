namespace Mvc_CRUD.Models;

    public class UserProfile
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string LastName { get; set; }
        public string UserId { get; set; }
        public string? UserProfilePicUrl { get; set; }
        public string? UserCoverPicUrl { get; set; }
        public string? Bio { get; set; }
        public string? Location { get; set; }
        public string? HighSchoolName { get; set; }
        public string? Subject { get; set; }
        public string? CollegeName { get; set; }
        public string? Course { get; set; }
        public string? RelationShipStatus { get; set; }
        public string? Website { get; set; }
        public string? FromLocation { get; set; }
        public string? SchoolPeriod { get; set; }
        public string? CollegePeriod { get; set; }
        public string? JobTitle { get; set; }
        public string? Industry { get; set; }
        public string? JobPeriod { get; set; }
    }

