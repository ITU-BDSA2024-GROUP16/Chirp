using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyChat.Razor;

namespace Chirp.Pages;

public class UserTimelineModel : PageModel
{
    private readonly ICheepRepository _repository;
    public List<Cheep> Cheeps { get; set; }
    private const int PageSize = 32;
    public int pageNumber { get; set; }

    public UserTimelineModel(ICheepRepository repository)
    {
        _repository = repository;
    }

    public async Task<ActionResult> OnGetAsync(string author)
    {
        //default to page number 1 if no page is specified
        var pageQuery = Request.Query["page"];
        pageNumber = int.TryParse(pageQuery, out int page) ? page : 1;
        
        Cheeps = await _repository.GetCheepsFromAuthor(author, pageNumber, PageSize);
        return Page();
    }
}
