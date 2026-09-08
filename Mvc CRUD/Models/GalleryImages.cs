namespace Mvc_CRUD.Models;

    public class GalleryImages
    {
    public int Id { get; set; }
    public string UserName { get; set; }
    public string LastName { get; set; }
    public string UserId { get; set; }
    public string ImageUrl { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }

