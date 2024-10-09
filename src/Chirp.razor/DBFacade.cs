using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Chirp;

public class DBFacade
{
    public static async Task<List<CheepViewModel>> LoadCheeps(int pageNumber)
    {
        var cheeps = new List<CheepViewModel>();
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
                cheeps.Add(new CheepViewModel(
                    cheep.Author.Name, // Assuming Author has a Username property
                    cheep.Text,
                    cheep.TimeStamp.ToString("MM/dd/yy H:mm:ss") // Format the DateTime as needed
                ));
            }
        }

        return cheeps; // Return the list of CheepViewModel objects
    }
}
