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
    public virtual DbSet<Likes> Like { get; set; }
    public virtual DbSet<Comments> Comment { get; set; }
    public virtual DbSet<CommentsReply> ReplyComments { get; set; }
    public virtual DbSet<ReplyOfReply> Replies { get; set; }
        public DataDbContext(DbContextOptions<DataDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
              
               modelBuilder.Entity<Likes>()
                .HasOne<Posts>()
                .WithMany(x => x.PostLikes)
                .HasForeignKey(k => k.PostId);

               modelBuilder.Entity<CommentsReply>()
                .HasOne(r => r.Comment)
                .WithMany(c => c.Reply)
                .HasForeignKey(r => r.CommentId);

               modelBuilder.Entity<ReplyOfReply>()
                .HasOne(r => r.CommentReplies)
                .WithMany(c => c.Replies)
                .HasForeignKey(r => r.ReplyId);

                //  *********** Database Indexes **************

               modelBuilder.Entity<Friends>()
                .HasIndex(f => f.UserId);

               modelBuilder.Entity<Friends>()
                 .HasIndex(f => new {f.UserName, f.FriendName});

               modelBuilder.Entity<Friends>()
                .HasIndex(f => new { f.FriendName, f.UserName});

               modelBuilder.Entity<Friends>()
                .HasIndex(f => new { f.UserId, f.FriendName});

               modelBuilder.Entity<Posts>()
                .HasIndex(p => new {p.PostScope, p.UserName });

               modelBuilder.Entity<Likes>()
                .HasIndex(x => new {x.PostId, x.IsDeleted});

               modelBuilder.Entity<UserProfile>()
                .HasIndex(x => x.UserId);

               modelBuilder.Entity<Chat>()
                .HasIndex(f => new {f.UserName, f.ToUser});
          
               modelBuilder.Entity<Chat>()
                .HasIndex(f => new {f.ToUser, f.UserName});


        }

}

