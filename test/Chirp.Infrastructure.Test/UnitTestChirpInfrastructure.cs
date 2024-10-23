using Chirp.Core;
using Chirp.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Assert = Xunit.Assert;

namespace Chirp.Infrastructure.Test;

public class UnitTestChirpInfrastructure : IAsyncLifetime
{
    private SqliteConnection _connection;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        await _connection.OpenAsync();
    }

    public async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
    }

    private CheepDBContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CheepDBContext>()
            .UseSqlite(_connection) 
            .Options;

        var context = new CheepDBContext(options);
        context.Database.EnsureCreated(); 
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
        await dbContext.SaveChangesAsync(); 

        var author = await _cheepRepository.FindAuthorWithName(_testAuthor.Name);

        Assert.NotNull(author);
        Assert.Equal(_testAuthor.Name, author.Name);
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
        await dbContext.SaveChangesAsync(); 

        var author = await _cheepRepository.FindAuthorWithEmail(_testAuthor.Email);

        Assert.NotNull(author);
        Assert.Equal(_testAuthor.Email, author.Email); 
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
        
        await dbContext.Authors.AddAsync(_testAuthor1);
        await dbContext.SaveChangesAsync(); 

        var _testAuthor2 = new Author
        {
            Name = "Test Name", 
            Email = "test@gmail.com", 
            Cheeps = new List<Cheep>(),
        };
        
        await Assert.ThrowsAsync<DbUpdateException>(async () =>
        {
            await dbContext.Authors.AddAsync(_testAuthor2);
            await dbContext.SaveChangesAsync(); 
        });
    }
}
