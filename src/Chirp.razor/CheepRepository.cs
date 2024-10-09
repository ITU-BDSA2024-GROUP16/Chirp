using MyChat.Razor;

namespace Chirp;


public interface ICheepRepository
{
    Task<List<Cheep>> GetCheeps(int pageNumber, int pageSize);
    Task<List<Cheep>> GetCheepsFromAuthor(string author, int pageNumber, int pageSize);
}

public class CheepRepository : ICheepRepository
{
    private readonly CheepDBContext _dbContext;
    public CheepRepository(CheepDBContext dbContext)
    {
        _dbContext = dbContext;
        SQLitePCL.Batteries.Init();
    }
    public async Task<List<Cheep>> GetCheeps(int pageNumber, int pageSize)
    {
        var cheeps = await DBFacade.LoadCheeps(pageNumber);
        return cheeps.ToList();
    }
    
    public async Task<List<Cheep>> GetCheepsFromAuthor(string author, int pageNumber, int pageSize)
    {
        var cheeps = await DBFacade.LoadCheeps(pageNumber);
        return cheeps.Where(x => x.Author.Name == author).ToList();
    }
}