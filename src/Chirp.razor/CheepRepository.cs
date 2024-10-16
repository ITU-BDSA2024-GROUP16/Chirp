
namespace Chirp;


public interface ICheepRepository
{
    List<Cheep> GetCheeps(int pageNumber, int pageSize);
    List<Cheep> GetCheepsFromAuthor(string author, int pageNumber, int pageSize);
}

public class CheepRepository : ICheepRepository
{
    private readonly DBFacade _dbFacade;
    public CheepRepository(DBFacade dbFacade)
    {
        _dbFacade = dbFacade;
        SQLitePCL.Batteries.Init();
    }
    public List<Cheep> GetCheeps(int pageNumber, int pageSize)
    {
        var cheeps = _dbFacade.LoadCheeps(pageNumber);
        return cheeps.ToList();
    }
    
    public List<Cheep> GetCheepsFromAuthor(string author, int pageNumber, int pageSize)
    {
        var cheeps = _dbFacade.LoadCheeps(pageNumber);
        return cheeps.Where(x => x.Author.Name == author).ToList();
    }
}