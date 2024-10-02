using Chirp;
using Microsoft.Data.Sqlite;

public record CheepViewModel(string Author, string Message, string Timestamp);

public interface ICheepService
{
    public List<CheepViewModel> GetCheeps(int pageNumber, int pageSize);
    public List<CheepViewModel> GetCheepsFromAuthor(string author, int pageNumber, int pageSize);
}

public class CheepService : ICheepService
{
    static CheepService()
    {
        // Initialize SQLite
        SQLitePCL.Batteries.Init();
    }
    // These would normally be loaded from a database for example
    //private static readonly List<CheepViewModel> _cheeps = DBFacade.LoadCheeps();
    

    public List<CheepViewModel> GetCheeps(int pageNumber, int pageSize)
    {
        return DBFacade.LoadCheeps(pageNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public List<CheepViewModel> GetCheepsFromAuthor(string author, int pageNumber, int pageSize)
    {
        // filter by the provided author name
        return DBFacade.LoadCheeps(pageNumber)
            .Where(x => x.Author == author)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }
/*
    private static string UnixTimeStampToDateTimeString(double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp);
        return dateTime.ToString("MM/dd/yy H:mm:ss");
    }
    */

}