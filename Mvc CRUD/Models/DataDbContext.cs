using Microsoft.EntityFrameworkCore;

namespace Mvc_CRUD.Models;

    public class DataDbContext : DbContext
    {

     public virtual DbSet<Chat> Chats { get; set; }
    public DataDbContext(DbContextOptions<DataDbContext> options) : base(options) 
    {

    }

    }

