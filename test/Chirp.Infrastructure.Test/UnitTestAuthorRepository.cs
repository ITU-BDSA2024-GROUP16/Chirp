using Chirp.Core;
using Chirp.Web;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;
using Assert = Xunit.Assert;


namespace Chirp.Infrastructure.Test;

public class UnitTestAuthorRepository : IAsyncLifetime
{
    private SqliteConnection? _connection;
    private readonly ITestOutputHelper _output;
    
    public UnitTestAuthorRepository(ITestOutputHelper output)
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
            Name = "Arthur",
            Email = "arthursadventures@Email.com"
        };
            
        dbContext.Authors.Add(testAuthor);
        await dbContext.SaveChangesAsync(); 

        var author = await authorRepository.FindAuthorWithName(testAuthor.Name);

        Assert.NotNull(author);
        Assert.Equal(testAuthor.Name, author.Name);
    }

    [Fact]
    public async Task UnitTestFindAuthorWithName_ThrowsExceptionIfAuthorIsNull()
    {
        await using var dbContext = CreateContext();
        var authorRepository = new AuthorRepository(dbContext );
        
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await authorRepository.FindAuthorWithName("NullAuthorName");
        });

        Assert.Equal("Author with name NullAuthorName not found.", exception.Message);
    }
    
    [Fact]
    public async Task UnitTestGetAuthorFromEmail()
    {
        await using var dbContext = CreateContext();
        var authorRepository = new AuthorRepository(dbContext );

        var testAuthor = new Author
        {
            Name = "Arthur",
            Email = "arthursadventures@Email.com"
        };

        dbContext.Authors.Add(testAuthor);
        await dbContext.SaveChangesAsync(); 

        var author = await authorRepository.FindAuthorWithEmail(testAuthor.Email);

        Assert.NotNull(author);
        Assert.Equal(testAuthor.Email, author.Email); 
    }
    
    [Fact]
    public async Task UnitTestFindAuthorWithEmail_ThrowsExceptionIfAuthorIsNull()
    {
        await using var dbContext = CreateContext();
        var authorRepository = new AuthorRepository(dbContext );
        
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await authorRepository.FindAuthorWithEmail("nullemail@gmail.com");
        });

        Assert.Equal($"Author with email nullemail@gmail.com not found.", exception.Message);
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
        };
        
        var testAuthor2 = new Author
        {
            AuthorId = 2,
            Name = "Clint",
            Email = "satanthedevil13@gmail.dk",
        };
        
        dbContext.Authors.Add(testAuthor1);
        dbContext.Authors.Add(testAuthor2);
        await dbContext.SaveChangesAsync();
        //Act - testAuthor1 follows testAuthor2
        await authorRepository.FollowUserAsync(testAuthor1.AuthorId, testAuthor2.AuthorId);
        
        //assert
        Assert.NotNull(testAuthor1);
        Assert.NotNull(testAuthor2);
        Assert.NotNull(testAuthor1.FollowedAuthors);
        Assert.NotNull(testAuthor2.Followers);
        Assert.True(await authorRepository.IsFollowingAsync(testAuthor1.AuthorId, testAuthor2.AuthorId));
        Assert.Contains(testAuthor1, testAuthor2.Followers);
        Assert.Contains(testAuthor2, testAuthor1.FollowedAuthors);

    }
    
    [Fact]
    public async Task UnitTestCannotFollowIfAlreadyFollowing()
    {
        await using var dbContext = CreateContext();
        var authorRepository = new AuthorRepository(dbContext);
        
        //arrange
        var testAuthor1 = new Author
        {
            AuthorId = 1,
            Name = "Delilah",
            Email = "angelfromabove4@gmail.dk",
        };
        
        var testAuthor2 = new Author
        {
            AuthorId = 2,
            Name = "Clint",
            Email = "satanthedevil13@gmail.dk",
        };
        
        dbContext.Authors.Add(testAuthor1);
        dbContext.Authors.Add(testAuthor2);
        await dbContext.SaveChangesAsync();
        //Act - testAuthor1 follows testAuthor2
        await authorRepository.FollowUserAsync(testAuthor1.AuthorId, testAuthor2.AuthorId);
        
        await authorRepository.FollowUserAsync(testAuthor1.AuthorId, testAuthor2.AuthorId);
        Assert.True(await authorRepository.IsFollowingAsync(testAuthor1.AuthorId, testAuthor2.AuthorId));
    }
    

    [Fact]
    public async Task UnitTestFollowUserAsync_ThrowsExceptionIfFollowerIsNull()
    {
        await using var dbContext = CreateContext();
        var authorRepository = new AuthorRepository(dbContext );
        
        var testAuthor = new Author
        {
            Name = "Poppy",
            Email = "seedsfor4life@gmail.dk",
        };
        dbContext.Authors.Add(testAuthor);
        await dbContext.SaveChangesAsync();
        
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await authorRepository.FollowUserAsync(9999999, testAuthor.AuthorId);
        });

        Assert.Equal("Follower or follower's name is null.", exception.Message);
        
    }
    [Fact]
    public async Task UnitTestFollowUserAsync_ThrowsExceptionIfFollowerNameIsNull()
    {
        await using var dbContext = CreateContext();
        var authorRepository = new AuthorRepository(dbContext );
        
        var testAuthor = new Author
        {
            Email = "seedsfor4life@gmail.dk",
        };
        dbContext.Authors.Add(testAuthor);
        await dbContext.SaveChangesAsync();
        
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await authorRepository.FollowUserAsync(testAuthor.AuthorId, 99999);
        });

        Assert.Equal("Follower or follower's name is null.", exception.Message);
        
    }
    
    [Fact]
    public async Task UnitTestFollowUserAsync_ThrowsExceptionIfFollowedIsNull()
    {
        await using var dbContext = CreateContext();
        var authorRepository = new AuthorRepository(dbContext );
        
        var testAuthor = new Author
        {
            Name = "Tassles",
            Email = "creationfromabove@gmail.dk",
        };
        
        dbContext.Authors.Add(testAuthor);
        await dbContext.SaveChangesAsync();
        
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await authorRepository.FollowUserAsync(testAuthor.AuthorId, 888888);
        });

        Assert.Equal("Followed author or followed author's name is null.", exception.Message);
        
    }
    
    [Fact]
    public async Task UnitTestFollowUserAsync_ThrowsExceptionIfFollowedNameIsNull()
    {
        await using var dbContext = CreateContext();
        var authorRepository = new AuthorRepository(dbContext );
        
        var testAuthor = new Author
        {
            AuthorId = 1,
            Name = "Grus",
            Email = "creationfromabove@gmail.dk",
        };
        
        var testAuthor2 = new Author
        {
            Email = "amongthewilds@gmail.dk",
        };
        
        dbContext.Authors.Add(testAuthor);
        await dbContext.SaveChangesAsync();
        
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await authorRepository.FollowUserAsync(testAuthor.AuthorId, testAuthor2.AuthorId);
        });

        Assert.Equal("Followed author or followed author's name is null.", exception.Message);
        
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
            FollowedAuthors = new List<Author>(),
            Followers = new List<Author>()

        };

        var testAuthor2 = new Author
        {
            AuthorId = 2,
            Name = "Clint",
            Email = "satanthedevil13@gmail.dk",
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

        List<Author> authors;

        authors = await authorRepository.SearchAuthorsAsync("jacq");
        
        Assert.Contains(authors, author => author.Name == "Jacqualine Gilcoine");

    }

    [Fact]
    public async Task WhenSearchingAuthorsIsEmptyCollection()
    {
        await using var dbContext = CreateContext();

        DbInitializer.SeedDatabase(dbContext);
        var authorRepository = new AuthorRepository(dbContext);

        List<Author> authors;

        authors = await authorRepository.SearchAuthorsAsync("12345567");
        
        Assert.Empty(authors);
    }

    
    
    [Fact]
    public async Task IfAuthorExistsReturnTrue()
    {
        await using var dbContext = CreateContext();
        DbInitializer.SeedDatabase(dbContext);
        var authorRepository = new AuthorRepository(dbContext);
        
        Author author = new Author()
        {
            Name = "Jacqie",
            Email = "jacque@itu.dk",
            AuthorId = 1000,
        };
        
        await dbContext.Authors.AddAsync(author);
        await dbContext.SaveChangesAsync();

        bool isAuthorFound = await authorRepository.FindIfAuthorExistsWithEmail(author.Email);
        
        Assert.True(isAuthorFound);

        
    }
    
    [Fact]
    public async Task IfAuthorDoesNotExistReturnFalse()
    {
        await using var dbContext = CreateContext();
        DbInitializer.SeedDatabase(dbContext);
        var authorRepository = new AuthorRepository(dbContext);
        
        bool isAuthorFound = await authorRepository.FindIfAuthorExistsWithEmail("CountCommint@itu.dk");
        
        Assert.False(isAuthorFound);

        
    }

    [Fact]
    public async Task UnitTestFindAuthorWithId()
    {
        await using var dbContext = CreateContext();
        DbInitializer.SeedDatabase(dbContext);
        var authorRepository = new AuthorRepository(dbContext);
        
        
        Author author = new Author()
        {
            Name = "Jacqie",
            Email = "jacque@itu.dk",
            AuthorId = 1000,
        };
        
        await dbContext.Authors.AddAsync(author);
        await dbContext.SaveChangesAsync();

        Author foundAuthor =  await authorRepository.FindAuthorWithId(1000);
        Assert.Equal(author, foundAuthor);

    }
    
    [Fact]
    public async Task UnitTestFindAuthorWithId_ThrowsExceptionIfAuthorIsNull()
    {
        await using var dbContext = CreateContext();
        var authorRepository = new AuthorRepository(dbContext );
        
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await authorRepository.FindAuthorWithId(999999999);
        });

        Assert.Equal("Author with ID 999999999 was not found.", exception.Message);
    }

    [Fact]
    public async Task UnitTestGetFollowing()
    {
        await using var dbContext = CreateContext();
        var authorRepository = new AuthorRepository(dbContext);
        
        var testAuthor1 = new Author
        {
            AuthorId = 1,
            Name = "Delilah",
            Email = "angelfromabove4@gmail.dk",
        };

        var testAuthor2 = new Author
        {
            AuthorId = 2,
            Name = "Clint",
            Email = "satanthedevil13@gmail.dk",
        };

        dbContext.Authors.Add(testAuthor1);
        dbContext.Authors.Add(testAuthor2);
        await dbContext.SaveChangesAsync();

        await authorRepository.FollowUserAsync(testAuthor1.AuthorId, testAuthor2.AuthorId);
        List<Author> author1Following = await authorRepository.getFollowing(testAuthor1.AuthorId);
        
        Assert.Contains(testAuthor2, author1Following);
    }
    
    [Fact]
    public async Task UnitTestGetFollowing_ThrowsExceptionIfFollowerIsNull()
    {
        await using var dbContext = CreateContext();
        var authorRepository = new AuthorRepository(dbContext);
        
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await authorRepository.getFollowing(999999);
        });

        Assert.Equal("Follower or followed authors is null.", exception.Message);

    }
    
}