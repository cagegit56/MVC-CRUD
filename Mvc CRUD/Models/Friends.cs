namespace Mvc_CRUD.Models;

    public class Friends
    {
       public int Id { get; set; }
       public string UserId { get; set; }
       public string FriendId { get; set; }
       public string Status { get; set; }
       public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }

