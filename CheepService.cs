using Chirp;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.IO;

public record CheepViewModel(string Author, string Message, string Timestamp);

public interface ICheepService
{
    List<CheepViewModel> GetCheeps();
    List<CheepViewModel> GetCheepsFromAuthor(string author);
}

public class CheepService : ICheepService
{
    static CheepService()
    {
        // Initialize SQLite
        SQLitePCL.Batteries.Init();

        // Initialize the database
        InitializeDatabase();
    }

    private static void InitializeDatabase()
    {
        // Get the database path from the environment variable or set to /tmp/chirp.db
        string dbPath = Environment.GetEnvironmentVariable("CHIRPDBPATH") ?? "/tmp/chirp.db";

        RunInitScript(dbPath);
    }


    private static void RunInitScript(string dbPath)
    {
        // Path to your initDB.sh script
        string initScriptPath = Path.Combine(Directory.GetCurrentDirectory(), "scripts", "initDB.sh");

        Console.WriteLine($"Init Script Path: {initScriptPath}");
        Console.WriteLine($"Database Path: {dbPath}");

        // Create the process start info
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"-c \"CHIRPDBPATH='{dbPath}' bash '{initScriptPath}'\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using (Process process = Process.Start(startInfo))
        {
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            Console.WriteLine("Output:");
            Console.WriteLine(output);
            Console.WriteLine("Error:");
            Console.WriteLine(error);

            if (process.ExitCode != 0)
            {
                Console.WriteLine("Error running init script:");
                Console.WriteLine(error);
                return;
            }

            Console.WriteLine("Database initialized successfully.");
        }
    }


    // These would normally be loaded from a database for example
    private static readonly List<CheepViewModel> _cheeps = DBFacade.LoadCheeps();

    public List<CheepViewModel> GetCheeps()
    {
        return _cheeps;
    }

    public List<CheepViewModel> GetCheepsFromAuthor(string author)
    {
        // filter by the provided author name
        return _cheeps.Where(x => x.Author == author).ToList();
    }
}
