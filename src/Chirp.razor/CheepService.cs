using Chirp;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

public record CheepViewModel(string Author, string Message, string Timestamp);

public interface ICheepService
{
    List<CheepViewModel> GetCheeps(int pageNumber, int pageSize);
    List<CheepViewModel> GetCheepsFromAuthor(string author, int pageNumber, int pageSize);
}

public class CheepService : ICheepService
{
    public CheepService()
    {
        // Initialize SQLite
        SQLitePCL.Batteries.Init();
    }
    
    public static void CreateDatabase()
    {
        // Get the path from the environment variable or default to /tmp
        string dbPath = Environment.GetEnvironmentVariable("CHIRPDBPATH") ?? "/tmp/chirp.db";
        
        // Get the current directory and build relative paths for the SQL files
        string baseDirectory = Directory.GetCurrentDirectory();
        string schemaFilePath = Path.Combine(baseDirectory, "data", "schema.sql");
        string dumpFilePath = Path.Combine(baseDirectory, "data", "dump.sql");

        // Check if running on Windows and convert paths for WSL if needed
        bool isWindows = Path.DirectorySeparatorChar == '\\';
        if (isWindows)
        {
            schemaFilePath = ConvertToWslPath(schemaFilePath);
            dumpFilePath = ConvertToWslPath(dumpFilePath);
        }

        // Check if files exist
        if (!File.Exists(schemaFilePath) || !File.Exists(dumpFilePath))
        {
            throw new FileNotFoundException("Schema or Dump file not found.");
        }

        // Initialize the database by executing the schema and dump files
        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            connection.Open();
            ExecuteSqlFile(schemaFilePath, connection);
            ExecuteSqlFile(dumpFilePath, connection);
        }

        Console.WriteLine($"Database initialized at {dbPath}.");
    }

    private static void ExecuteSqlFile(string filePath, SqliteConnection connection)
    {
        string sql = File.ReadAllText(filePath);
        using (var command = new SqliteCommand(sql, connection))
        {
            command.ExecuteNonQuery();
        }
    }

    private static string ConvertToWslPath(string windowsPath)
    {
        // Convert Windows path to WSL-compatible path
        return windowsPath.Replace('\\', '/').Replace("C:/", "/mnt/c/");
    }

    public List<CheepViewModel> GetCheeps(int pageNumber, int pageSize)
    {
        return DBFacade.LoadCheeps(pageNumber)
            .ToList();
    }

    public List<CheepViewModel> GetCheepsFromAuthor(string author, int pageNumber, int pageSize)
    {
        // filter by the provided author name
        return DBFacade.LoadCheeps(pageNumber)
            .Where(x => x.Author == author)
            .ToList();
    }
}
