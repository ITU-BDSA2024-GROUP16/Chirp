namespace Chirp
{
    public interface ICheepRepository
    {
        List<CheepDTO> GetCheeps(int pageNumber, int pageSize);
        List<Cheep> GetCheepsFromAuthor(string author, int pageNumber, int pageSize);
    }

    public class CheepRepository : ICheepRepository
    {
        private readonly DBFacade _dbFacade;

        public CheepRepository(DBFacade dbFacade)
        {
            _dbFacade = dbFacade;
            SQLitePCL.Batteries.Init();
        }

        public List<CheepDTO> GetCheeps(int pageNumber, int pageSize)
        {
            var cheeps = _dbFacade.LoadCheeps(pageNumber);

            return cheeps.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(cheep => new CheepDTO
                {
                    AuthorName = cheep.Author.Name, 
                    Text = cheep.Text,               
                    Timestamp = cheep.TimeStamp.ToString("g") 
                })
                .ToList();
        }

        public List<Cheep> GetCheepsFromAuthor(string author, int pageNumber, int pageSize)
        {
            var cheeps = _dbFacade.LoadCheeps(pageNumber);

            return cheeps.Where(x => x.Author.Name == author).ToList();
        }
    }
}