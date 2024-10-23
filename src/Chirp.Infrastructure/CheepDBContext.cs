using Chirp.Core;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure
{
    public class CheepDBContext : DbContext
    {
        public DbSet<Cheep> Cheeps { get; set; }
        public DbSet<Author> Authors { get; set; }

        public CheepDBContext(DbContextOptions<CheepDBContext> dbContextOptions)
            : base(dbContextOptions)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Author>()
                .HasIndex(a => a.Name)
                .IsUnique(); // Ensures unique names

            modelBuilder.Entity<Author>()
                .HasIndex(a => a.Email)
                .IsUnique(); // Ensures unique emails

            base.OnModelCreating(modelBuilder);
        }
    }
}