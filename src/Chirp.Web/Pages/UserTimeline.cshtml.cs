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
    
    public async Task<ActionResult> OnPost(string Message, string author)
    {
        
        Author authorPlaceHolder = await _cheepRepository.FindAuthorWithEmail(author);
        
        if (authorPlaceHolder == null)
        {
            Console.WriteLine("Author not found for email: " + author);
            // Return an appropriate error response or create a new author if necessary.
            ModelState.AddModelError(string.Empty, "Author not found.");
            return Page();
        }
        
        Console.WriteLine("This is the author " + author);
        Console.WriteLine("This is the auhtorPlaceHolder " + authorPlaceHolder.Name);
        
        var cheep = new Cheep
        {
            AuthorId = authorPlaceHolder.AuthorId,
            Text = Message,
            TimeStamp = DateTime.Now,
            Author = authorPlaceHolder
        };
        //authorPlaceHolder.Cheeps.Add(cheep);
        await _cheepRepository.SaveCheep(cheep);
        return RedirectToPage();
    }
}
