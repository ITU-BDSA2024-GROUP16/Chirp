using Chirp.Core;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure
{
    public interface IAuthorRepository
    {
        Task<Author> FindAuthorWithName(string userName);
        Task<Author> FindAuthorWithEmail(string email);
        Task<bool> IsFollowingAsync(int followerId, int followedId);
        Task<List<Author>> GetFollowing(int followerId);
        Task<bool> FindIfAuthorExistsWithEmail(string email);
        Task FollowUserAsync(int followerId, int followedId);
        Task UnFollowUserAsync(int followerId, int followedId);
        Task<List<Cheep>> GetLikedCheeps(int userId);
        Task<List<AuthorDTO>> SearchAuthorsAsync(string searchWord);
    }

    public class AuthorRepository : IAuthorRepository
    {
        public readonly CheepDBContext DbContext;

        public AuthorRepository(CheepDBContext dbContext)
        {
            DbContext = dbContext;
            SQLitePCL.Batteries.Init();
        }
        

        public async Task<Author> FindAuthorWithName(string userName)
        {
            var author = await DbContext.Authors
                .Include(a => a.FollowedAuthors!)
                .ThenInclude(fa => fa.Cheeps)
                .Include(a => a.Cheeps)
                .Include(a => a.Followers)
                .Include(a => a.LikedCheeps)
                .AsSplitQuery()
                .FirstOrDefaultAsync(author => author.Name == userName);
            if (author == null)
            {
                throw new InvalidOperationException($"Author with name {userName} not found.");
            }

            return author;
        }

        public async Task<Author> FindAuthorWithEmail(string email)
        {
            var author = await DbContext.Authors.FirstOrDefaultAsync(author => author.Email == email);
            if (author == null)
            {
                throw new InvalidOperationException($"Author with email {email} not found.");
            }

            return author;
        }
        
        public async Task<bool> FindIfAuthorExistsWithEmail(string email)
        {
            var author = await DbContext.Authors.FirstOrDefaultAsync(author => author.Email == email);
            if (author == null)
            {
                return false;
            }

            return true;
        }
        
        public async Task<Author> FindAuthorWithId(int authorId)
        {
            var author = await DbContext.Authors.FirstOrDefaultAsync(author => author.AuthorId == authorId);
            if (author == null)
            {
                throw new InvalidOperationException($"Author with ID {authorId} was not found.");
            }

            return author;
        }
        
        

        public async Task FollowUserAsync(int followerId, int followedId)
        {
            //logged in user
            var follower = await DbContext.Authors.SingleOrDefaultAsync(a => a.AuthorId == followerId);
            //the user that the logged in user wants to follow
            var followed = await DbContext.Authors.SingleOrDefaultAsync(a => a.AuthorId == followedId);
            
            if (follower == null || follower.Name == null)
            {
                throw new InvalidOperationException("Follower or follower's name is null.");
            }

            if (followed == null || followed.Name == null)
            {
                throw new InvalidOperationException("Followed author or followed author's name is null.");
            }
            
            if (!await IsFollowingAsync(followerId, followedId))
            {
                follower.FollowedAuthors?.Add(followed);
                followed.Followers?.Add(follower);
                await DbContext.SaveChangesAsync();
            }
        }

        public async Task UnFollowUserAsync(int followerId, int followedId)
        {
            // The logged in Author
            var follower = await DbContext.Authors
                .Include(a => a.FollowedAuthors) 
                .AsSplitQuery()
                .SingleOrDefaultAsync(a => a.AuthorId == followerId);
        
            // The author whom the logged in author is unfollowing
            var followed = await DbContext.Authors
                .SingleOrDefaultAsync(a => a.AuthorId == followedId);

            if (follower != null && followed != null)
            {
                if (follower.FollowedAuthors?.Contains(followed) == true)
                {
                    follower.FollowedAuthors.Remove(followed);
                    await DbContext.SaveChangesAsync();
                }
            }
        }
        
        public async Task<bool> IsFollowingAsync(int followerId, int followedId)
        {
            var loggedInUser = await DbContext.Authors.Include(a => a.FollowedAuthors)
                .Include(a => a.FollowedAuthors)
                .AsSplitQuery()
                .FirstOrDefaultAsync(a => a.AuthorId == followerId);

            return loggedInUser?.FollowedAuthors?.Any(f => f.AuthorId == followedId) ?? false;
        }

        public async Task<List<Author>> GetFollowing(int followerId)
        {
            var follower = await DbContext.Authors.Include(a => a.FollowedAuthors)
                .Include(a => a.FollowedAuthors)
                .AsSplitQuery()
                .FirstOrDefaultAsync(a => a.AuthorId == followerId);
            if (follower == null || follower.FollowedAuthors == null)
            {
                throw new InvalidOperationException("Follower or followed authors is null.");
            }
            return follower.FollowedAuthors;
        }

        public async Task<List<Cheep>> GetLikedCheeps(int userId)
        {
            var user = await DbContext.Authors
                .Include(a => a.LikedCheeps)
                .AsSplitQuery()
                .FirstOrDefaultAsync(a => a.AuthorId == userId);
            // user.LikedCheeps cannot be null here because the query ensures it's at least an empty list
            // we added the check so the compiler doesn't give us a warning in the return statement
            if (user == null || user.LikedCheeps == null)
            {
                throw new InvalidOperationException("User liked cheeps is null.");
            }
            return user.LikedCheeps;
        }
        

        public async Task<List<AuthorDTO>> SearchAuthorsAsync(string searchWord)
        {
            if (string.IsNullOrWhiteSpace(searchWord))
            {
                return new List<AuthorDTO>(); // Return empty list if no search word is provided
            }

            if (searchWord.Length > 2)
            {
                // Perform a case-insensitive search for authors whose name contains the search word
                return await DbContext.Authors
                    .Where(a => EF.Functions.Like(a.Name, $"%{searchWord}%"))
                    .Select(a => new AuthorDTO
                    {
                        Name = a.Name // Map Author entity to AuthorDTO
                    })
                    .ToListAsync();
            }
            else
            {
                return await DbContext.Authors
                    .Where(a => EF.Functions.Like(a.Name, $"{searchWord}%"))
                    .Select(a => new AuthorDTO
                    {
                        Name = a.Name // Map Author entity to AuthorDTO
                    })
                    .ToListAsync();
            }
        }
    }
}