namespace Mvc_CRUD.Models;

    public class Friends
    {
       public int Id { get; set; }
       public string UserId { get; set; }
       public string UserName { get; set; }
       public string FriendId { get; set; }
       public string FriendName { get; set; }
       public string Status { get; set; }
       public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
       public virtual Chat_Users? AllUsers { get; set; }
    }

