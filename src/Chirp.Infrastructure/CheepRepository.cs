using Chirp.Core;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure
{
    public interface ICheepRepository
    {
        Task<List<CheepDTO>> GetCheeps(int pageNumber, int pageSize);
        Task SaveCheep(Cheep cheep, Author author);
        Task<List<Cheep>> GetCheepsByAuthor(int authorId);
        Task<bool> DoesUserLikeCheep(Cheep cheep, Author author);
        Task<Cheep> CreateCheepFromCheepDto(CheepDTO cheepDto, Author author);
        Task LikeCheep(Cheep cheep, Author author);
        Task UnLikeCheep(Cheep cheep, Author author);
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
        
        public async Task<Cheep> CreateCheepFromCheepDto(CheepDTO cheepDto, Author author)
        {
            if (cheepDto == null)
            {
                throw new ArgumentNullException(nameof(cheepDto), "CheepDTO cannot be null.");
            }

            if (string.IsNullOrEmpty(cheepDto.TimeStamp))
            {
                throw new ArgumentException("Timestamp is required.", nameof(cheepDto.TimeStamp));
            }

            if (!DateTime.TryParse(cheepDto.TimeStamp, out var parsedTimeStamp))
            {
                throw new FormatException("Invalid timestamp format.");
            }

            return new Cheep
            {
                Author = author,
                AuthorId = author.AuthorId,
                Text = cheepDto.Text,
                TimeStamp = parsedTimeStamp,
                Likes = cheepDto.Likes ?? 0 // Default to 0 if Likes is null
            };
        }


        public async Task<bool> DoesUserLikeCheep(Cheep cheep, Author author)
        {
            if (author.LikedCheeps == null)
            {
                return false;
            }
            
            foreach (var likedCheep in author.LikedCheeps)
            {
                if (cheep.Text == likedCheep.Text)
                {
                    return true;
                }
            }
            return false;
        }
        
        public async Task LikeCheep(Cheep cheep, Author author)
        {
            if (author.LikedCheeps != null)
            {
                author.LikedCheeps.Add(cheep);
            }
            await _dbContext.SaveChangesAsync();
        }
        
        public async Task UnLikeCheep(Cheep cheep, Author author)
        {
            if (author.LikedCheeps != null)
            {
                var cheepsToRemove = author.LikedCheeps
                    .Where(c => c.Text == cheep.Text)
                    .ToList(); 
                
                foreach (var cheepToRemove in cheepsToRemove)
                {
                    author.LikedCheeps.Remove(cheepToRemove);
                }
            }
            
            await _dbContext.SaveChangesAsync();
        }

    }
}