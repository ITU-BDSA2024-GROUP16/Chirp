using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Pages;

public class PublicModel : PageModel
{
    private readonly ICheepRepository _cheepRepository;
    public List<Cheep> Cheeps { get; set; }
    private const int PageSize = 32;
    public int pageNumber { get; set; }
    

    public PublicModel(ICheepRepository cheepRepository)
    {
        _cheepRepository = cheepRepository;
    }

    public ActionResult OnGet()
    {
        //default to page number 1 if no page is specified
        var pageQuery = Request.Query["page"];
        pageNumber = int.TryParse(pageQuery, out int page) ? page : 1;
        
        Cheeps = _cheepRepository.GetCheeps(pageNumber, PageSize);
        return Page();
    }
}
