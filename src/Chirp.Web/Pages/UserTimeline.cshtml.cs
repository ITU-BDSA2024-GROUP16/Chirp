using System.ComponentModel.DataAnnotations;
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
    [BindProperty]
    [StringLength(160, ErrorMessage = "Cheep cannot be more than 160 characters.")]
    public string Text { get; set; }


    public UserTimelineModel(ICheepRepository cheepRepository)
    {
        _cheepRepository = cheepRepository;
    }

    public async Task<ActionResult> OnGet()
    {
        //default to page number 1 if no page is specified
        var pageQuery = Request.Query["page"];
        PageNumber = int.TryParse(pageQuery, out int page) ? page : 1;
        
        //var authorName = User.FindFirst("Name")?.Value;
        var authorName = HttpContext.GetRouteValue("author").ToString();
        
        Author author = await _cheepRepository.FindAuthorWithName(authorName);
        
        List<CheepDTO> cheeps = author.Cheeps
            .OrderByDescending(cheep => cheep.TimeStamp)
            .Select(cheep => new CheepDTO
        {
            Author = cheep.Author != null ? cheep.Author.Name : "Unknown",
            Text = cheep.Text,
            TimeStamp = cheep.TimeStamp.ToString("g")
        }).ToList();

        Cheeps = cheeps;

        //Cheeps = await _cheepRepository.ReadCheeps(author, PageNumber, PageSize);
        return Page();
    }
    
    public async Task<ActionResult> OnPost()
    {
        var authorName = User.FindFirst("Name")?.Value;
        
        Author author = await _cheepRepository.FindAuthorWithName(authorName);
        
        var cheep = new Cheep
        {
            AuthorId = author.AuthorId,
            Text = Text,
            TimeStamp = DateTime.Now,
            Author = author
        };
        
        await _cheepRepository.SaveCheep(cheep, author);
        
        return RedirectToPage();
    }
}