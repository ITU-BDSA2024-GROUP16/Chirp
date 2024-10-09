using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MyChat.Razor;

namespace Chirp;

public class DBFacade
{
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
/*
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
        */
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
    public static async Task<List<Cheep>> LoadCheeps(int pageNumber)
    {
        
        
        var cheeps = new List<Cheep>();
        // Get the database path from the environment variable or default to /tmp/chirp.db
        var sqlDBFilePath = Environment.GetEnvironmentVariable("CHIRPDBPATH") ?? "/tmp/chirp.db"; 

        var optionsBuilder = new DbContextOptionsBuilder<CheepDBContext>();
        optionsBuilder.UseSqlite($"Data Source={sqlDBFilePath}");

        // Ensure that CheepDBContext is not nullable
        using (var dbContext = new CheepDBContext(optionsBuilder.Options))
        {
            // LINQ query using EF Core
            var query = (from cheep in dbContext.Cheeps
                    orderby cheep.TimeStamp descending
                    select cheep)
                .Include(c => c.Author)
                .Skip(pageNumber * 32)
                .Take(32);

            var result = await query.ToListAsync();

            // Map results to CheepViewModel
            foreach (var cheep in result)
            {
                cheeps.Add(cheep);
                
            }
        }

        return cheeps; // Return the list of CheepViewModel objects
    }
}
