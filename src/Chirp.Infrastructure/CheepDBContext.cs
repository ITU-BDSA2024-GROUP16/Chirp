using Chirp.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure
{
    public class CheepDBContext : IdentityDbContext<Author, IdentityRole<int>, int>
    {
        public DbSet<Cheep> Cheeps { get; set; }
        public DbSet<Author> Authors { get; set; }

        public CheepDBContext(DbContextOptions<CheepDBContext> dbContextOptions)
            : base(dbContextOptions)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure unique indexes for Author properties
            modelBuilder.Entity<Author>()
                .HasIndex(a => a.Name)
                .IsUnique();
            modelBuilder.Entity<Author>()
                .HasIndex(a => a.Email)
                .IsUnique();
            
            // Configure many-to-many relationship for FollowedAuthors
            modelBuilder.Entity<Author>()
                .HasMany(a => a.FollowedAuthors)
                .WithMany(a => a.Followers)
                .UsingEntity<Dictionary<string, object>>(
                    "AuthorFollows",
                    j => j.HasOne<Author>().WithMany().HasForeignKey("FollowedId").OnDelete(DeleteBehavior.Restrict),
                    j => j.HasOne<Author>().WithMany().HasForeignKey("FollowerId").OnDelete(DeleteBehavior.Cascade));
            
            // Configure maximum length for Cheep text
            modelBuilder.Entity<Cheep>().Property(c => c.Text).HasMaxLength(160);

            base.OnModelCreating(modelBuilder);
         
                
        }
    }
}