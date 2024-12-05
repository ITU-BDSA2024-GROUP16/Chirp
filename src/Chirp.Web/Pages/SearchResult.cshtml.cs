using Chirp.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chirp.Infrastructure;

namespace Chirp.Web.Pages
{
    public class SearchResultsModel : PageModel
    {
        private readonly IAuthorRepository _authorRepository;
        [BindProperty(SupportsGet = true)]
        public string? SearchWord { get; set; }
        public List<AuthorDTO> AuthorDTOs { get; set; } = new List<AuthorDTO>();

        
        
        public SearchResultsModel(IAuthorRepository authorRepository)
        {
            _authorRepository = authorRepository;
        }
        public async Task OnGet()
        {
            if (!string.IsNullOrEmpty(SearchWord))
            {
                AuthorDTOs = await _authorRepository.SearchAuthorsAsync(SearchWord);
                
            }
        }
    }
}