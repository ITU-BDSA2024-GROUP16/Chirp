using Chirp.Core;
using Chirp.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Assert = Xunit.Assert;

namespace Chirp.Infrastructure.Test;

public class UnitTestChirpInfrastructure : IAsyncLifetime
{
    private string _dbFileName;
    private string _dbFilePath;

    public async Task InitializeAsync()
    {
        var tempDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tmp");

        if (!Directory.Exists(tempDir))
        {
            Directory.CreateDirectory(tempDir);
        }

        // Create a unique database file name
        _dbFileName = $"myChirpDatabase_{Guid.NewGuid()}.db";
        _dbFilePath = Path.Combine(tempDir, _dbFileName);
    }

    public async Task DisposeAsync()
    {
        // Delete the database file only if it exists
        if (File.Exists(_dbFilePath))
        {
            try
            {
                File.Delete(_dbFilePath);
            }
            catch (IOException)
            {
                // Handle the exception or log it if needed
            }
        }
    }

    private CheepDBContext CreateContext()
    {
        var connectionString = $"Filename={_dbFilePath}";
        var connection = new SqliteConnection(connectionString);
        connection.Open();

        var options = new DbContextOptionsBuilder<CheepDBContext>()
            .UseSqlite(connection)
            .Options;

        var context = new CheepDBContext(options);
        context.Database.EnsureCreated(); // Create the database if not exists
        return context;
    }

    [Fact]
    public async Task UnitTestGetAuthorFromName()
    {
        await using var dbContext = CreateContext();
        var _cheepRepository = new CheepRepository(new DBFacade(dbContext), dbContext);

        var _testAuthor = new Author
        {
            Name = "Test Name",
            Email = "test@gmail.com",
            Cheeps = new List<Cheep>(),
        };

        dbContext.Authors.Add(_testAuthor);
        await dbContext.SaveChangesAsync(); // Make sure to save changes

        var author = await _cheepRepository.FindAuthorWithName(_testAuthor.Name);

        Assert.NotNull(author);
        Assert.Equal(_testAuthor.Name, author.Name); // Compare names
    }

    [Fact]
    public async Task UnitTestGetAuthorFromEmail()
    {
        await using var dbContext = CreateContext();
        var _cheepRepository = new CheepRepository(new DBFacade(dbContext), dbContext);

        var _testAuthor = new Author
        {
            Name = "Test Name",
            Email = "test@gmail.com",
            Cheeps = new List<Cheep>(),
        };

        dbContext.Authors.Add(_testAuthor);
        await dbContext.SaveChangesAsync(); // Make sure to save changes

        var author = await _cheepRepository.FindAuthorWithEmail(_testAuthor.Email);

        Assert.NotNull(author);
        Assert.Equal(_testAuthor.Email, author.Email); // Compare emails
    }

    [Fact]
    public async Task UnitTestGetNonexistingAuthor()
    {
        await using var dbContext = CreateContext();
        var _cheepRepository = new CheepRepository(new DBFacade(dbContext), dbContext);

        var author = await _cheepRepository.FindAuthorWithName("DrDontExist");

        Assert.Null(author);
    }
    
    [Fact]
    public async Task UnitTestDuplicateAuthors()
    {
        await using var dbContext = CreateContext();
        var _cheepRepository = new CheepRepository(new DBFacade(dbContext), dbContext);
        
        var _testAuthor1 = new Author
        {
            Name = "Test Name",
            Email = "test@gmail.com",
            Cheeps = new List<Cheep>(),
        };
        
        // Add the first author to the context
        await dbContext.Authors.AddAsync(_testAuthor1);
        await dbContext.SaveChangesAsync(); // Save changes to commit the first author

        var _testAuthor2 = new Author
        {
            Name = "Test Name", // Same name
            Email = "test@gmail.com", // Same email
            Cheeps = new List<Cheep>(),
        };

        // Use Assert.ThrowsAsync to check if an exception is thrown
        await Assert.ThrowsAsync<DbUpdateException>(async () =>
        {
            await dbContext.Authors.AddAsync(_testAuthor2);
            await dbContext.SaveChangesAsync(); // This should throw an exception
        });
    }
}
