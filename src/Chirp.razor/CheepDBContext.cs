using Microsoft.EntityFrameworkCore;
using MyChat.Razor;

namespace Chirp;

public class CheepDBContext : DbContext
{
    DbSet<Cheep> Cheeps { get; set; } 
    private DbSet<Author> Authors { get; set; }

    public CheepDBContext(DbContextOptions<CheepDBContext> dbContextOptions)
        : base(dbContextOptions)
    {
        
    }
}