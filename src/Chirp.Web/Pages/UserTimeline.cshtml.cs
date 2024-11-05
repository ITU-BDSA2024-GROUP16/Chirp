using System.Security.Claims;
using Chirp.Core;
using Chirp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace Chirp.Web.Pages;

public class UserTimelineModel : PageModel
{
    private readonly ICheepRepository _cheepRepository;
    public List<CheepDTO> Cheeps { get; set; } = new List<CheepDTO>();
    private const int PageSize = 32;
    public int PageNumber { get; set; }


    public UserTimelineModel(ICheepRepository cheepRepository)
    {
        _cheepRepository = cheepRepository;
    }

    public async Task<ActionResult> OnGet(string author)
    {
        //default to page number 1 if no page is specified
        var pageQuery = Request.Query["page"];
        PageNumber = int.TryParse(pageQuery, out int page) ? page : 1;

        Cheeps = await _cheepRepository.ReadCheeps(author, PageNumber, PageSize);
        return Page();
    }
    
    public async Task<ActionResult> OnPost(string Message)
    {
        // author may not be needed, when you can find name with claim.
        //Author authorPlaceHolder = await _cheepRepository.FindAuthorWithName(author);
        
        
        var authorName = User.FindFirst(ClaimTypes.Name)?.Value;
        
        Author author = await _cheepRepository.FindAuthorWithEmail(authorName);
        
        var cheep = new Cheep
        {
            AuthorId = author.AuthorId,
            Text = Message,
            TimeStamp = DateTime.Now,
            Author = author
        };
        
        await _cheepRepository.SaveCheep(cheep);
        author.Cheeps.Add(cheep);
        
        return RedirectToPage();
    }
}
