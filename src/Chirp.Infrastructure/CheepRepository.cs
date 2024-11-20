using Chirp.Core;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure
{
    public interface ICheepRepository
    {
        Task<List<CheepDTO>> GetCheeps(int pageNumber, int pageSize);
        Task<List<CheepDTO>> ReadCheeps(string userName, int pageNumber, int pageSize);
        Task<int> CreateCheep(CheepDTO dto);
        Task<Author> FindAuthorWithName(string userName);
        Task<Author> FindAuthorWithEmail(string email);
        Task CreateAuthor(string name, string email);
        Task SaveCheep(Cheep cheep, Author author);
        Task UpdateAuthorAsync(Author author);
        Task<List<Cheep>> GetCheepsByAuthor(int authorId);
        Task<bool> IsFollowingAsync(int followerId, int followedId);
        Task<List<Author>> getFollowing(int followerId);
        Task FollowUserAsync(int followerId, int followedId);
        Task UnFollowUserAsync(int followerId, int followedId);
    }

    public class CheepRepository : ICheepRepository
    {
        private readonly DBFacade _dbFacade;
        public readonly CheepDBContext _dbContext;

        public CheepRepository(DBFacade dbFacade, CheepDBContext dbContext)
        {
            _dbFacade = dbFacade;
            _dbContext = dbContext;
            SQLitePCL.Batteries.Init();
        }

        public async Task<List<Cheep>> GetCheepsByAuthor(int authorId)
        {
            return await _dbContext.Cheeps
                .Where(c => c.AuthorId == authorId)
                .OrderByDescending(c => c.TimeStamp)
                .ToListAsync();
        }

        public async Task<List<CheepDTO>> GetCheeps(int pageNumber, int pageSize)
        {
            var cheeps = _dbContext.Cheeps;

            var cheepsQuery = await cheeps.OrderByDescending(cheep => cheep.TimeStamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(cheep => new CheepDTO
                {
                    Author = cheep.Author != null ? cheep.Author.Name : "Unknown",
                    Text = cheep.Text,
                    TimeStamp = cheep.TimeStamp.ToString("g")
                })
                .ToListAsync();
            return cheepsQuery;
        }

        public async Task<List<CheepDTO>> ReadCheeps(string userName, int pageNumber, int pageSize)
        {
            var query = _dbContext.Cheeps.Select(cheep => new { cheep.Author, cheep.Text, cheep.TimeStamp });

            var result = await query.ToListAsync();

            return result
                .Where(cheep => cheep.Author != null && cheep.Author.Name == userName)
                .OrderByDescending(cheep => cheep.TimeStamp) // Use userName for filtering
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(cheep => new CheepDTO
                {
                    Author = cheep.Author != null ? cheep.Author.Name : "Unknown",
                    Text = cheep.Text,
                    TimeStamp = cheep.TimeStamp.ToString("g")
                })
                .ToList();
        }


        public async Task<int> CreateCheep(CheepDTO dto)
        {
            Cheep newCheep = new()
            {
                Text = dto.Text,
                Author = new Author() { Name = dto.Author },
                TimeStamp = DateTime.TryParse(dto.TimeStamp, out var dateTime) ? dateTime : DateTime.Now

            };
            var queryResult = await _dbContext.Cheeps.AddAsync(newCheep);

            await _dbContext.SaveChangesAsync();
            return queryResult.Entity.CheepId;
        }

        public async Task<Author> FindAuthorWithName(string userName)
        {
            var author = await _dbContext.Authors
                .Include(a => a.Cheeps)
                .FirstOrDefaultAsync(author => author.Name == userName);
            // var author = await _dbContext.Authors.FirstOrDefaultAsync(author => author.Name == userName);
            if (author == null)
            {
                throw new InvalidOperationException($"Author with name {userName} not found.");
            }

            return author;
        }

        public async Task<Author> FindAuthorWithEmail(string email)
        {
            var author = await _dbContext.Authors.FirstOrDefaultAsync(author => author.Email == email);
            if (author == null)
            {
                throw new InvalidOperationException($"Author with email {email} not found.");
            }

            return author;
        }
        
        public async Task<Author> FindAuthorWithId(int authorId)
        {
            var author = await _dbContext.Authors.FirstOrDefaultAsync(author => author.AuthorId == authorId);
            if (author == null)
            {
                throw new InvalidOperationException($"Author with ID {authorId} was not found.");
            }

            return author;
        }

        public async Task CreateAuthor(string name, string email)
        {
            Author author = new Author
            {
                Name = name,
                Email = email,
                AuthorId = _dbContext.Authors.Count() + 1,
                Cheeps = new List<Cheep>()
            };

            try
            {
                _dbContext.Authors.Add(author);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is Microsoft.Data.Sqlite.SqliteException sqliteEx &&
                    sqliteEx.SqliteErrorCode == 19)
                {
                    Console.WriteLine("User Already exists");
                }
            }
        }

        public async Task SaveCheep(Cheep cheep, Author author)
        {
            await _dbContext.Cheeps.AddAsync(cheep);
            await _dbContext.SaveChangesAsync();

            await _dbContext.Entry(author).Collection(a => a.Cheeps).LoadAsync();
        }

        public async Task UpdateAuthorAsync(Author author)
        {
            _dbContext.Authors.Update(author);
            await _dbContext.SaveChangesAsync();
        }

        public async Task FollowUserAsync(int followerId, int followedId)
        {
            //logged in user
            var follower = await _dbContext.Authors.SingleOrDefaultAsync(a => a.AuthorId == followerId);
            //the user that the logged in user wants to follow
            var followed = await _dbContext.Authors.SingleOrDefaultAsync(a => a.AuthorId == followedId);
            
            Console.WriteLine("Logged in author: " + follower.Name + "author wants to follow: " + followed.Name);
            
            
            if (!await IsFollowingAsync(followerId, followedId))
            {
                follower.FollowedAuthors.Add(followed);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task UnFollowUserAsync(int followerId, int followedId)
        {
            //logged in user
            var follower = await _dbContext.Authors.SingleOrDefaultAsync(a => a.AuthorId == followerId);
            //the user that the logged in user wants to follow
            var followed = await _dbContext.Authors.SingleOrDefaultAsync(a => a.AuthorId == followedId);

            Console.WriteLine("hejsa");
            if (follower != null && followed != null)
            {
                Console.WriteLine("hej");
                follower.FollowedAuthors.Remove(followed);
                await _dbContext.SaveChangesAsync();
            }
        }


        public async Task<bool> IsFollowingAsync(int followerId, int followedId)
        {
            var loggedInUser = await _dbContext.Authors.Include(a => a.FollowedAuthors)
                .FirstOrDefaultAsync(a => a.AuthorId == followerId);

            return loggedInUser?.FollowedAuthors.Any(f => f.AuthorId == followedId) ?? false;
        }

        public async Task<List<Author>> getFollowing(int followerId)
        {
            var follower = await _dbContext.Authors.Include(a => a.FollowedAuthors)
                .FirstOrDefaultAsync(a => a.AuthorId == followerId);
            return follower.FollowedAuthors;
        }

}
}