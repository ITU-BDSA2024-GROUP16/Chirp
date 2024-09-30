
using System.Data;
using Microsoft.Data.Sqlite;
public record CheepViewModel(string Author, string Message, string Timestamp);

public interface ICheepService
{
    public List<CheepViewModel> GetCheeps();
    public List<CheepViewModel> GetCheepsFromAuthor(string author);
}

public class CheepService : ICheepService
{
    static CheepService()
    {
        // Initialize SQLite
        SQLitePCL.Batteries.Init();
    }
    // These would normally be loaded from a database for example
    private static readonly List<CheepViewModel> _cheeps = LoadCheeps();

    private static List<CheepViewModel> LoadCheeps()
    {
        var cheeps = new List<CheepViewModel>();
        var sqlDBFilePath = "/tmp/chirp.db"; 

        using (var connection = new SqliteConnection($"Data Source={sqlDBFilePath}"))
        {
            connection.Open();
            
           // var sqlQuery = "SELECT author_id, message_id, pub_date FROM message ORDER BY pub_date DESC"; // Adjust to match your table structure
            var sqlQuery = "SELECT u.username, m.author_id, m.text, m.pub_date FROM user u JOIN message m ON u.user_id = m.author_id ORDER BY m.pub_date DESC";
            using (var command = new SqliteCommand(sqlQuery, connection))
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    long pubDateUnix = reader.GetInt64(reader.GetOrdinal("pub_date"));
                    DateTime pubDate = DateTimeOffset.FromUnixTimeSeconds(pubDateUnix).DateTime;

                    // Create CheepViewModel and add to the list
                    var cheep = new CheepViewModel(
                        reader.GetString(reader.GetOrdinal("username")),
                        reader.GetString(reader.GetOrdinal("text")),
                        pubDate.ToString("MM/dd/yy H:mm:ss")
                        //reader.GetDateTime(reader.GetOrdinal("pub_date")).ToString("MM/dd/yy H:mm:ss")
                    );

                    cheeps.Add(cheep); // Add the created CheepViewModel to the list
                }
            }
        }

        return cheeps; // Return the list of CheepViewModel objects
    }


    public List<CheepViewModel> GetCheeps()
    {
        return _cheeps;
    }

    public List<CheepViewModel> GetCheepsFromAuthor(string author)
    {
        // filter by the provided author name
        return _cheeps.Where(x => x.Author == author).ToList();
    }

    private static string UnixTimeStampToDateTimeString(double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp);
        return dateTime.ToString("MM/dd/yy H:mm:ss");
    }

}