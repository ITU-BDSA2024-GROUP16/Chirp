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

            // Configure the relationship between Cheep and Author (one-to-many)
            modelBuilder.Entity<Cheep>()
                .HasOne(c => c.Author)  // Cheep has one Author
                .WithMany(a => a.Cheeps)  // Author has many Cheeps
                .HasForeignKey(c => c.AuthorId) // Foreign key in Cheep pointing to Author
                .OnDelete(DeleteBehavior.Cascade);  // Set delete behavior

            // Many-to-many relationship for LikedCheeps (Cheep can have many Authors who liked it)
            modelBuilder.Entity<Cheep>()
                .HasMany(c => c.LikedByAuthors) // Cheep has many Authors who liked it
                .WithMany(a => a.LikedCheeps)  // Author can like many Cheeps
                .UsingEntity(j => j.ToTable("AuthorLikedCheeps"));  // Join table

            // Many-to-many relationship for FollowedAuthors
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