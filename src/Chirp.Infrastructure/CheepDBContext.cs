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
}