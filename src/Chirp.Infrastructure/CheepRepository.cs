using Chirp.Core;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure
{
    public interface ICheepRepository
    {
        Task<List<CheepDTO>> GetCheeps(int pageNumber, int pageSize);
        Task SaveCheep(Cheep cheep, Author author);
        Task<List<Cheep>> GetCheepsByAuthor(int authorId);
    }

    public class CheepRepository : ICheepRepository
    {
        public readonly CheepDBContext _dbContext;

        public CheepRepository(CheepDBContext dbContext)
        {
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
        

        public async Task SaveCheep(Cheep cheep, Author author)
        {
            await _dbContext.Cheeps.AddAsync(cheep);
            await _dbContext.SaveChangesAsync();

            await _dbContext.Entry(author).Collection(a => a.Cheeps).LoadAsync();
        }

    }
}