namespace Mvc_CRUD.Models;

    public class BlockedUsers
    {
       public int Id { get; set; }
       public string UserId { get; set; }
       public string BlockUserId { get; set; } 
       public string UserName {  get; set; } 
       public string BlockUserName { get; set; }
       public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
       public bool IsDeleted { get; set; } = false;

    }

