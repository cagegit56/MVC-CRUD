using Mvc_CRUD.Models;

namespace Mvc_CRUD.Dto;

    public class ChatsDto
    {
    public int Id { get; set; }
    public string UserName { get; set; }
    public string ToUser { get; set; }
    public string Message { get; set; }
    public DateTime SentOn { get; set; }
    public virtual Chat_Users? AllUsers { get; set; }
    public virtual Friends? Friend {  get; set; }
}

