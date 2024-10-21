using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure
{
    public interface ICheepRepository
    {
        Task<List<CheepDTO>> GetCheeps(int pageNumber, int pageSize);
        Task<List<CheepDTO>> ReadCheeps(string userName, int pageNumber, int pageSize);
        Task<int> CreateCheep(CheepDTO dto);
        Task<int> UpdateCheep(CheepDTO dto);
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
                    Author = cheep.Author.Name,
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
                .Where(cheep => cheep.Author.Name == userName) // Use userName for filtering
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(cheep => new CheepDTO
                {
                    Author = cheep.Author.Name,
                    Text = cheep.Text,
                    TimeStamp = cheep.TimeStamp.ToString("g")
                })
                .ToList();
        }

        
        public async Task<int> CreateCheep(CheepDTO dto)
        {
            Cheep newCheep = new()
            {
                Text = dto.Text, Author = new Author() {Name = dto.Author}, TimeStamp = DateTime.Parse(dto.TimeStamp)
            };
            var queryResult = await _dbContext.Cheeps.AddAsync(newCheep); 

            await _dbContext.SaveChangesAsync(); 
            return queryResult.Entity.CheepId;
        }
        
        public async Task<int> UpdateCheep(CheepDTO dto)
        {
            // we have not finished this method
            Cheep newCheep = new()
            {
                Text = dto.Text, Author = new Author() {Name = dto.Author}, TimeStamp = DateTime.Parse(dto.TimeStamp)
            };
            var queryResult = await _dbContext.Cheeps.AddAsync(newCheep); 

            await _dbContext.SaveChangesAsync(); 
            return queryResult.Entity.CheepId;
        }
        
        
    }
}