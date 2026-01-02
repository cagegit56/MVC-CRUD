using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Mvc_CRUD.Models;

public class DataDbContext : DbContext
{
    public virtual DbSet<Chat> Chats { get; set; }
    public virtual DbSet<Chat_Users> ChatUsers { get; set; }
    public virtual DbSet<Friends> Friends { get; set; }
    public virtual DbSet<FriendRequest> FriendRequests { get; set; }
    public virtual DbSet<BlockedUsers> BlockedUser { get; set; }
    public virtual DbSet<UserProfile> Profile { get; set; }
    public virtual DbSet<Posts> Post { get; set; }
    public virtual DbSet<Comments> Comment { get; set; }
        public DataDbContext(DbContextOptions<DataDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

               modelBuilder.Entity<Comments>()
                .HasOne<Posts>()
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId);


            //modelBuilder.Entity<Friends>()
            //    .HasOne(c => c.AllUsers)
            //    .WithMany()
            //    .HasForeignKey(c => c.FriendId)
            //    .HasPrincipalKey(u => u.UserId);

            //modelBuilder.Entity<Friends>()
            //    .HasMany(c => c.FriendRequests)
            //    .WithOne()
            //    .HasForeignKey(c => c.UserId)
            //    .HasPrincipalKey(u => u.UserId)
            //    .IsRequired(false);

            //modelBuilder.Entity<Friends>()
            //    .HasOne(c => c.BlockedUser)
            //    .WithMany()
            //    .HasForeignKey(c => c.UserId)
            //    .HasPrincipalKey(u => u.UserId)
            //    .IsRequired(false);

        }
        

    }

