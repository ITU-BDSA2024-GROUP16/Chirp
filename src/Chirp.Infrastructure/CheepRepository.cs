using Chirp.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Caching.Distributed;

namespace Chirp.Infrastructure
{
    public interface ICheepRepository
    {
        Task<List<CheepDTO>> GetCheeps(int pageNumber, int pageSize);
        Task SaveCheep(Cheep cheep, Author author);
        Task<List<Cheep>> GetCheepsByAuthor(int authorId);
        Task<bool> DoesUserLikeCheep(Cheep cheep, Author author);
        Task LikeCheep(Cheep cheep, Author author);
        Task UnLikeCheep(Cheep cheep, Author author);
        Task<Cheep?> FindCheep(string text, string timestamp, string authorName);
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
                    AuthorName = cheep.Author != null ? cheep.Author.Name : "Unknown",
                    Text = cheep.Text,
                    TimeStamp = cheep.TimeStamp.ToString(),
                    Likes = cheep.Likes,
                })
                .ToListAsync();
            return cheepsQuery;
        }
        

        public async Task SaveCheep(Cheep cheep, Author author)
        {
            if (author.Cheeps == null)
            {
                throw new InvalidOperationException("Author's Cheeps collection is null.");
            }
            
            await _dbContext.Cheeps.AddAsync(cheep);
            await _dbContext.SaveChangesAsync();
            await _dbContext.Entry(author).Collection(a => a.Cheeps!).LoadAsync();
        }
        
        public Task<bool> DoesUserLikeCheep(Cheep cheep, Author author)
        {
            if (author.LikedCheeps == null)
            {
                return Task.FromResult(false);
            }

            foreach (var likedCheep in author.LikedCheeps)
            {
                if (cheep.Text == likedCheep.Text)
                {
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }

        
        public async Task LikeCheep(Cheep cheep, Author author)
        {
            if (author.LikedCheeps != null)
            {
                author.LikedCheeps.Add(cheep);
                cheep.Likes += 1;
            }
            await _dbContext.SaveChangesAsync();
        }
        
        public async Task UnLikeCheep(Cheep cheep, Author author)
        {
            if (author.LikedCheeps != null)
            {
                author.LikedCheeps.Remove(cheep);
                cheep.Likes -= 1;
            }
            
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Cheep?> FindCheep(string text, string timestamp, string authorName)
        {
            if (!DateTime.TryParse(timestamp, out var parsedTimestamp))
            {
                throw new ArgumentException("Invalid timestamp format.");
            }
            
            var cheeps = await _dbContext.Cheeps
                .Include(c => c.Author)
                .ToListAsync();
            
            foreach (var cheep in cheeps)
            {
                if (cheep == null || cheep.Author == null || string.IsNullOrWhiteSpace(cheep.Text) || string.IsNullOrWhiteSpace(cheep.Author.Name))
                {
                    throw new ArgumentException("Cheep was not found");
                }
                if (cheep.Text.ToLower() == text.ToLower() && cheep.TimeStamp.ToString().ToLower() == parsedTimestamp.ToString().ToLower() && cheep.Author != null && cheep.Author.Name.ToLower() == authorName.ToLower())
                {
                    return cheep;
                }
            }

            return null;
        }
    }
}