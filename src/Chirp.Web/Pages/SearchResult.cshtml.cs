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
        [BindProperty(SupportsGet = true)]
        public string? SearchWord { get; set; }
        public List<Author> Authors { get; set; } = new List<Author>();

        
        
        public SearchResultsModel(IAuthorRepository authorRepository)
        {
            _authorRepository = authorRepository;
        }
        public async Task OnGet()
        {
            if (!string.IsNullOrEmpty(SearchWord))
            {
                // Fetch authors filtered by the search word
                Authors = await _authorRepository.SearchAuthorsAsync(SearchWord);
            }
        }
        
        
    }
}