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
        Task SaveCheep(Cheep cheep);
    }

    public class CheepRepository : ICheepRepository
    {
        private readonly DBFacade _dbFacade;
        private readonly CheepDBContext _dbContext;

        public CheepRepository(DBFacade dbFacade, CheepDBContext dbContext)
        {
            _dbFacade = dbFacade;
            _dbContext = dbContext;
            SQLitePCL.Batteries.Init();
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
                .Where(cheep => cheep.Author != null && cheep.Author.Name == userName) // Use userName for filtering
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
                Author = new Author() {Name = dto.Author}, 
                TimeStamp = DateTime.TryParse(dto.TimeStamp, out var dateTime) ? dateTime : DateTime.Now
                
            };
            var queryResult = await _dbContext.Cheeps.AddAsync(newCheep); 

            await _dbContext.SaveChangesAsync(); 
            return queryResult.Entity.CheepId;
        }

        public async Task<Author> FindAuthorWithName(string userName)
        {
            var author = await _dbContext.Authors.FirstOrDefaultAsync(author => author.Name == userName);
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
                throw new InvalidOperationException($"Author with name {email} not found.");
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

        public async Task SaveCheep(Cheep cheep)
        {
            await _dbContext.Cheeps.AddAsync(cheep);
            await _dbContext.SaveChangesAsync();
        }
    }
}