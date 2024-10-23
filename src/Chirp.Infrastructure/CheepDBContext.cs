using Chirp.Core;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure;

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
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Author>()
            .HasIndex(c => c.Name)
            .IsUnique();
        modelBuilder.Entity<Author>()
            .HasIndex(c => c.Email)
            .IsUnique();
        modelBuilder.Entity<Cheep>();
        
        modelBuilder.Entity<Cheep>().Property(c => c.Text).HasMaxLength(160);
    }
}