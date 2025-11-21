using System.ComponentModel.DataAnnotations.Schema;

namespace Mvc_CRUD.Models;

    public class Chat
    {
       public int Id { get; set; }
       public string UserName { get; set; }
       public string ToUser { get; set; }   
       public string Message { get; set; }
       public DateTime SentOn { get; set; } = DateTime.UtcNow;
       
    }

