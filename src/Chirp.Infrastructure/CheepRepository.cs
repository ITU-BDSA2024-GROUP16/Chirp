using Chirp.Core;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure
{
    public interface ICheepRepository
    {
        Task<List<CheepDTO>> GetCheeps(int pageNumber, int pageSize);
        Task SaveCheep(Cheep cheep, Author author);
        Task<List<Cheep>> GetCheepsByAuthor(int authorId);
        Task<Cheep> GetCheepFromCheepDto(CheepDTO cheepDto);
        Task<bool> DoesUserLikeCheep(Cheep cheep, Author author);
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
                .Where(c => c.AuthorId == authorId) // Use the updated property name here
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
                    AuthorDTO = cheep.Author != null ? cheep.Author.Name : "Unknown",
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
            
            if (author.Cheeps == null)
            {
                throw new InvalidOperationException("Author's Cheeps collection is null.");
            }

            await _dbContext.Entry(author).Collection(a => a.Cheeps!).LoadAsync();
        }
        
        public async Task<Cheep> GetCheepFromCheepDto(CheepDTO cheepDto)
        {
            var cheeps = await _dbContext.Cheeps.ToListAsync();
            
            foreach(Cheep cheep in cheeps)
            {
                if (cheep.Text == cheepDto.Text)
                {
                    return cheep;
                }
            }
            return null;
        }

        public async Task<bool> DoesUserLikeCheep(Cheep cheep, Author author)
        {
            if (author.LikedCheeps != null)
            {
                return author.LikedCheeps.Contains(cheep);
            }
            else
            {
                return false;
            }
        }
        
        
    }
}