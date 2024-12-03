using Chirp.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Chirp.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace Chirp.Web.Pages
{
    public class SearchResultsModel : PageModel
    {
        private readonly IAuthorRepository _authorRepository;
        public readonly SignInManager<Author> _signInManager;
        public List<Author> followedAuthors { get; set; } = new List<Author>();
        string SearchText { get; set; }
        


        public SearchResultsModel(IAuthorRepository authorRepository, SignInManager<Author> signInManager)
        {
            _authorRepository = authorRepository;
            _signInManager = signInManager;
        }

        [BindProperty(SupportsGet = true)]
        public string SearchWord { get; set; }

        public List<Author> Authors { get; set; } = new List<Author>();

        public async Task OnGet()
        {
            if (!string.IsNullOrEmpty(SearchWord))
            {
                // Fetch authors filtered by the search word
                Authors = await _authorRepository.SearchAuthorsAsync(SearchWord);
                SearchText = SearchWord;
                Console.WriteLine("THIS IS SEARCHTEXT " + SearchText);
            }
        }
        
        public async Task<ActionResult> OnPostFollow(string followAuthorName)
        {
            //Finds the author thats logged in
            var authorName = User.FindFirst(ClaimTypes.Name)?.Value;
            var author = await _authorRepository.FindAuthorWithEmail(authorName);
        
            //Finds the author that the logged in author wants to follow
            var followAuthor = await _authorRepository.FindAuthorWithName(followAuthorName);
        
            await _authorRepository.FollowUserAsync(author.AuthorId, followAuthor.AuthorId);
        
            //updates the current author's list of followed authors
            followedAuthors = await _authorRepository.getFollowing(author.AuthorId);

            return new RedirectToPageResult("/SearchResults", new { SearchWord = SearchText });
        }
        
        
        public async Task<ActionResult> OnPostUnfollow(string followAuthorName)
        {
            //Finds the author thats logged in
            var authorName = User.FindFirst(ClaimTypes.Name)?.Value;
            var author = await _authorRepository.FindAuthorWithEmail(authorName);
        
            //Finds the author that the logged in author wants to follow
            var followAuthor = await _authorRepository.FindAuthorWithName(followAuthorName);
        
            await _authorRepository.UnFollowUserAsync(author.AuthorId, followAuthor.AuthorId);
        
            //updates the current author's list of followed authors
            followedAuthors = await _authorRepository.getFollowing(author.AuthorId);
        
            Console.WriteLine("Number of followed authors" + followedAuthors.Count);

            return RedirectToPage("/SearchResults", "jacq");
        }
    }
}