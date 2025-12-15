using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using connectly.Models;

namespace connectly.Data
{ 
    // Moștenește IdentityDbContext<ApplicationUser> conform Curs 9 
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<FollowRequest> FollowRequests => Set<FollowRequest>();
        public DbSet<Group> Groups => Set<Group>();
        public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
        public DbSet<GroupMessage> GroupMessages => Set<GroupMessage>();

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<FollowRequest>()
                .HasIndex(f => new { f.FromUserId, f.ToUserId })
                .IsUnique();

            builder.Entity<GroupMember>()
                .HasIndex(gm => new { gm.GroupId, gm.UserId })
                .IsUnique();

            builder.Entity<GroupMessage>()
                .HasOne(m => m.Group)
                .WithMany(g => g.Messages)
                .HasForeignKey(m => m.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Group>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(g => g.CreatedById)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<GroupMessage>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
