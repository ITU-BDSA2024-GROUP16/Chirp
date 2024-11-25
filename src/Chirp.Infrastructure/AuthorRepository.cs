using Chirp.Core;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure
{
    public interface IAuthorRepository
    {
        Task<Author> FindAuthorWithName(string userName);
        Task<Author> FindAuthorWithEmail(string email);
        Task<bool> IsFollowingAsync(int followerId, int followedId);
        Task<List<Author>> getFollowing(int followerId);
        Task FollowUserAsync(int followerId, int followedId);
        Task UnFollowUserAsync(int followerId, int followedId);
    }

    public class AuthorRepository : IAuthorRepository
    {
        public readonly CheepDBContext _dbContext;

        public AuthorRepository(CheepDBContext dbContext)
        {
            _dbContext = dbContext;
            SQLitePCL.Batteries.Init();
        }
        

        public async Task<Author> FindAuthorWithName(string userName)
        {
            var author = await _dbContext.Authors
                .Include(a => a.FollowedAuthors)
                .ThenInclude(fa => fa.Cheeps)
                .Include(a => a.Cheeps)
                .FirstOrDefaultAsync(author => author.Name == userName);
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