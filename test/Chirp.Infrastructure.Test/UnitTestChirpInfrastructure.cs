using Chirp.Core;
using Chirp.Infrastructure;
using Chirp.Web;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;
using Assert = Xunit.Assert;

namespace Chirp.Infrastructure.Test;

public class UnitTestChirpInfrastructure : IAsyncLifetime
{
    private SqliteConnection _connection;
    private readonly ITestOutputHelper _output;
    
    public UnitTestChirpInfrastructure(ITestOutputHelper output)
    {
        _output = output; // Assigning the output to the private field
    }

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
/*
    [Fact]
    public async Task UnitTestGetNonexistingAuthor()
    {
        await using var dbContext = CreateContext();
        var _cheepRepository = new CheepRepository(new DBFacade(dbContext), dbContext);

        var author = await _cheepRepository.FindAuthorWithName("DrDontExist");

        Assert.Null(author);
    }
    */
    
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

    [Fact]
    public async Task UnitTestTestPageSize()
    {
        //Arrange
        await using var dbContext = CreateContext();
        DbInitializer.SeedDatabase(dbContext);
        var cheepRepository = new CheepRepository(new DBFacade(dbContext), dbContext);
        
        List<CheepDTO> cheeps = new List<CheepDTO>();
        List<CheepDTO> cheeps2 = new List<CheepDTO>();
        
        //Act
        cheeps = await cheepRepository.GetCheeps(1, 32);
        cheeps2 = await cheepRepository.GetCheeps(1, 12);
        
        _output.WriteLine("cheeps: {0}, cheeps2: {1}", cheeps.Count, cheeps2.Count);

        //Assert
        Assert.Equal(32, cheeps.Count);
        Assert.Equal(12, cheeps2.Count);
    }
    [Fact]
    public async Task UnitTestGetCheepsFromAuthor()
    {
        //Arrange
        await using var dbContext = CreateContext();
        DbInitializer.SeedDatabase(dbContext);
        
        var cheepRepository = new CheepRepository(new DBFacade(dbContext), dbContext);
        List<CheepDTO> cheeps = new List<CheepDTO>();
        string AuthorName = "Jacqualine Gilcoine";
        
        //Act
        cheeps = await cheepRepository.ReadCheeps(AuthorName, 1,32);

        
        //Assert
        foreach (CheepDTO cheep in cheeps)
        {
            _output.WriteLine("cheep Author: {0}", cheep.Author);
            Assert.Equal(AuthorName, cheep.Author);
        }
    }

    [Fact]
    public async Task UnitTestNoAuthorNameDuplicates()
    {
        await using var dbContext = CreateContext();
        DbInitializer.SeedDatabase(dbContext);
        
        var _testAuthor1 = new Author
        {
            Name = "Jacqualine Gilcoine",
            Email = "test@gmail.com",
            Cheeps = new List<Cheep>(),
        };
        
        await Assert.ThrowsAsync<DbUpdateException>(async () =>
        {
            await dbContext.Authors.AddAsync(_testAuthor1);
            await dbContext.SaveChangesAsync(); 
        });
    }

    [Fact]
    public async Task UnitTestNoEmailDuplicates()
    {
        await using var dbContext = CreateContext();
        DbInitializer.SeedDatabase(dbContext);

        var _testAuthor1 = new Author
        {
            Name = "Jacqie Gilcoine",
            Email = "Jacqualine.Gilcoine@gmail.com",
            Cheeps = new List<Cheep>(),
        };
        
        await Assert.ThrowsAsync<DbUpdateException>(async () =>
        {
            await dbContext.Authors.AddAsync(_testAuthor1);
            await dbContext.SaveChangesAsync(); 
        });
    }
    
    [Fact]
    public async Task UnitTestTestNoCheepsOnEmptyPage()
    {
        //Arrange
        await using var dbContext = CreateContext();
        DbInitializer.SeedDatabase(dbContext);
        var cheepRepository = new CheepRepository(new DBFacade(dbContext), dbContext);
        
        List<CheepDTO> cheeps = new List<CheepDTO>();
        
        //Act
        cheeps = await cheepRepository.GetCheeps(100000, 32);
        
        _output.WriteLine("cheeps: {0}", cheeps.Count);

        //Assert
        Assert.Empty(cheeps);
    }
}
