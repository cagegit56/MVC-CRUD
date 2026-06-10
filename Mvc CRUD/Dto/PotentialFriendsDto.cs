using Mvc_CRUD.Models;

namespace Mvc_CRUD.Dto;

    public class PotentialFriendsDto
    {
       public string? Error { get; set; }
       public ICollection<UserProfile>? ProfileInfo { get; set; }
    }

