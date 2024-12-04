using Chirp.Core;
using Chirp.Web;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;
using Assert = Xunit.Assert;

namespace Chirp.Infrastructure.Test;

public class UnitTestChirpInfrastructure : IAsyncLifetime
{
    private SqliteConnection? _connection;
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
        if (_connection != null)
            {
                await _connection.DisposeAsync();
            }
    }

    private CheepDBContext CreateContext()
    {
        if (_connection == null)
        {
            throw new InvalidOperationException("Connection is null.");
        }
    
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
        var authorRepository = new AuthorRepository(dbContext );

        var testAuthor = new Author
        {
            Name = "Test Name",
            Email = "test@gmail.com",
            Cheeps = new List<Cheep>(),
        };

        dbContext.Authors.Add(testAuthor);
        await dbContext.SaveChangesAsync(); 

        var author = await authorRepository.FindAuthorWithName(testAuthor.Name);

        Assert.NotNull(author);
        Assert.Equal(testAuthor.Name, author.Name);
    }

    [Fact]
    public async Task UnitTestGetAuthorFromEmail()
    {
        await using var dbContext = CreateContext();
        var authorRepository = new AuthorRepository(dbContext );


        var testAuthor = new Author
        {
            Name = "Test Name",
            Email = "test@gmail.com",
            Cheeps = new List<Cheep>(),
        };

        dbContext.Authors.Add(testAuthor);
        await dbContext.SaveChangesAsync(); 

        var author = await authorRepository.FindAuthorWithEmail(testAuthor.Email);

        Assert.NotNull(author);
        Assert.Equal(testAuthor.Email, author.Email); 
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
        
        var testAuthor1 = new Author
        {
            Name = "Test Name",
            Email = "test@gmail.com",
            Cheeps = new List<Cheep>(),
        };
        
        await dbContext.Authors.AddAsync(testAuthor1);
        await dbContext.SaveChangesAsync(); 

        var testAuthor2 = new Author
        {
            Name = "Test Name", 
            Email = "test@gmail.com", 
            Cheeps = new List<Cheep>(),
        };
        
        await Assert.ThrowsAsync<DbUpdateException>(async () =>
        {
            await dbContext.Authors.AddAsync(testAuthor2);
            await dbContext.SaveChangesAsync(); 
        });
    }

    [Fact]
    public async Task UnitTestTestPageSize()
    {
        //Arrange
        await using var dbContext = CreateContext();
        DbInitializer.SeedDatabase(dbContext);
        var cheepRepository = new CheepRepository( dbContext);
        
        //Act
        List<CheepDTO> cheeps = await cheepRepository.GetCheeps(1, 32);
        List<CheepDTO> cheeps2 = await cheepRepository.GetCheeps(1, 12);
        
        _output.WriteLine("cheeps: {0}, cheeps2: {1}", cheeps.Count, cheeps2.Count);

        //Assert
        Assert.Equal(32, cheeps.Count);
        Assert.Equal(12, cheeps2.Count);
    }
    /*
    [Fact]
    public async Task UnitTestGetCheepsFromAuthor()
    {
        //Arrange
        await using var dbContext = CreateContext();
        DbInitializer.SeedDatabase(dbContext);
        
        List<CheepDTO> cheeps = new List<CheepDTO>();
        string authorName = "Jacqualine Gilcoine";
        
        //Act
        //cheeps = await cheepRepository.ReadCheeps(AuthorName, 1,32);

        
        //Assert
        foreach (CheepDTO cheep in cheeps)
        {
            _output.WriteLine("cheep Author: {0}", cheep.AuthorDTO);
            Assert.Equal(authorName, cheep.AuthorDTO);
        }
    }

*/
    [Fact]
    public async Task UnitTestNoAuthorNameDuplicates()
    {
        await using var dbContext = CreateContext();
        DbInitializer.SeedDatabase(dbContext);
        
        var testAuthor1 = new Author
        {
            Name = "Jacqualine Gilcoine",
            Email = "test@gmail.com",
            Cheeps = new List<Cheep>(),
        };
        
        await Assert.ThrowsAsync<DbUpdateException>(async () =>
        {
            await dbContext.Authors.AddAsync(testAuthor1);
            await dbContext.SaveChangesAsync(); 
        });
    }

    [Fact]
    public async Task UnitTestNoEmailDuplicates()
    {
        await using var dbContext = CreateContext();
        DbInitializer.SeedDatabase(dbContext);

        var testAuthor1 = new Author
        {
            Name = "Jacqie Gilcoine",
            Email = "Jacqualine.Gilcoine@gmail.com",
            Cheeps = new List<Cheep>(),
        };
        
        await Assert.ThrowsAsync<DbUpdateException>(async () =>
        {
            await dbContext.Authors.AddAsync(testAuthor1);
            await dbContext.SaveChangesAsync(); 
        });
    }
    
    [Fact]
    public async Task UnitTestTestNoCheepsOnEmptyPage()
    {
        //Arrange
        await using var dbContext = CreateContext();
        DbInitializer.SeedDatabase(dbContext);
        var cheepRepository = new CheepRepository(dbContext);

        List<CheepDTO> cheeps;
        
        //Act
        cheeps = await cheepRepository.GetCheeps(100000, 32);
        
        _output.WriteLine("cheeps: {0}", cheeps.Count);

        //Assert
        Assert.Empty(cheeps);
    }

    [Fact]

    public async Task UnitTestAddedToFollowersAndFollowedAuthorsWhenFollowing()
    {
        await using var dbContext = CreateContext();
        var authorRepository = new AuthorRepository(dbContext);
        
        //arrange
        var testAuthor1 = new Author
        {
            AuthorId = 1,
            Name = "Delilah",
            Email = "angelfromabove4@gmail.dk",
            Cheeps = new List<Cheep>(),
            FollowedAuthors = new List<Author>(),
            Followers = new List<Author>()

        };
        
        var testAuthor2 = new Author
        {
            AuthorId = 2,
            Name = "Clint",
            Email = "satanthedevil13@gmail.dk",
            Cheeps = new List<Cheep>(),
            FollowedAuthors = new List<Author>(),
            Followers = new List<Author>()
        };
        
        dbContext.Authors.Add(testAuthor1);
        dbContext.Authors.Add(testAuthor2);
        await dbContext.SaveChangesAsync();
        //Act - testAuthor1 follows testAuthor2
        await authorRepository.FollowUserAsync(testAuthor1.AuthorId, testAuthor2.AuthorId);
        
        //assert
        Assert.Contains(testAuthor1, testAuthor2.Followers);
        Assert.Contains(testAuthor2, testAuthor1.FollowedAuthors);

    }

    [Fact]
    public async Task UnitTestRemovedFromFollowersAndFollowedAuthorsWhenUnFollowing()
    {
        await using var dbContext = CreateContext();
        var authorRepository = new AuthorRepository(dbContext);

        //arrange
        var testAuthor1 = new Author
        {
            AuthorId = 1,
            Name = "Delilah",
            Email = "angelfromabove4@gmail.dk",
            Cheeps = new List<Cheep>(),
            FollowedAuthors = new List<Author>(),
            Followers = new List<Author>()

        };

        var testAuthor2 = new Author
        {
            AuthorId = 2,
            Name = "Clint",
            Email = "satanthedevil13@gmail.dk",
            Cheeps = new List<Cheep>(),
            FollowedAuthors = new List<Author>(),
            Followers = new List<Author>()
        };

        dbContext.Authors.Add(testAuthor1);
        dbContext.Authors.Add(testAuthor2);
        await dbContext.SaveChangesAsync();

        //Act - testAuthor1 follows testAuthor2
        await authorRepository.FollowUserAsync(testAuthor1.AuthorId, testAuthor2.AuthorId);
        //testAuthor1 unfollows testAuthor2
        await authorRepository.UnFollowUserAsync(testAuthor1.AuthorId, testAuthor2.AuthorId);

        //assert
        Assert.DoesNotContain(testAuthor1, testAuthor2.Followers);
        Assert.DoesNotContain(testAuthor2, testAuthor1.FollowedAuthors);
    }

    [Fact]
    public async Task WhenSearchingAuthorsCorrectAuthorsAreInList()
    {
        
        await using var dbContext = CreateContext();
        DbInitializer.SeedDatabase(dbContext);
        var authorRepository = new AuthorRepository(dbContext);
        
        List<Author> authors = new List<Author>();

        authors = await authorRepository.SearchAuthorsAsync("jacq");
        
        Assert.Contains(authors, author => author.Name == "Jacqualine Gilcoine");

    }

    [Fact]
    public async Task WhenSearchingAuthorsIsEmptyCollection()
    {
        await using var dbContext = CreateContext();

        DbInitializer.SeedDatabase(dbContext);
        var authorRepository = new AuthorRepository(dbContext);
        
        List<Author> authors = new List<Author>();

        authors = await authorRepository.SearchAuthorsAsync("12345567");
        
        Assert.Empty(authors);

    }
}
