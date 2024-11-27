using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Chirp.Core;
using Chirp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace Chirp.Web.Pages;

public class UserTimelineModel : PageModel
{
    public readonly IAuthorRepository _authorRepository;
    public readonly ICheepRepository _cheepRepository;
    public List<CheepDTO> Cheeps { get; set; } = new List<CheepDTO>();
    private const int PageSize = 32;
    public int PageNumber { get; set; }
    [BindProperty]
    [StringLength(160, ErrorMessage = "Cheep cannot be more than 160 characters.")]
    public string Text { get; set; }
    public List<Author> followedAuthors { get; set; } = new List<Author>();



    public UserTimelineModel(ICheepRepository cheepRepository, IAuthorRepository authorRepository)
    {
        _cheepRepository = cheepRepository;
        _authorRepository = authorRepository;
    }

    public async Task<ActionResult> OnGet()
    {
        //Gets the authorName from the currently LOGGED IN user
        var authorName = User.FindFirst("Name")?.Value ?? "User";
        //Gets the author name from the URL.
        var pageUser = HttpContext.GetRouteValue("author").ToString();

        // This checks if the logged in user's USERNAME equals to the value from the UserTimeline URL
        if (authorName == pageUser)
        {
            // Keeps track of which page the user is currently at
            var pageQuery = Request.Query["page"];
            PageNumber = int.TryParse(pageQuery, out int page) ? page : 1;
            
            //Loads the author with their cheeps and followers using the authors name
            Author author = await _authorRepository.FindAuthorWithName(authorName);

            //Creates a list to gather the author and all its followers
            var allAuthors = new List<Author> { author };
            //Adds all the followers to the list
            allAuthors.AddRange(author.FollowedAuthors);

            //Sorts and converts the cheeps into cheepdto
            List<CheepDTO> cheeps = allAuthors
                .SelectMany(a => a.Cheeps)
                .OrderByDescending(cheep => cheep.TimeStamp)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .Select(cheep => new CheepDTO
                {
                    AuthorDTO = cheep.Author != null ? cheep.Author.Name : "Unknown",
                    Text = cheep.Text,
                    TimeStamp = cheep.TimeStamp.ToString("g")
                })
                .ToList();

            // Assign the combined list to Cheeps
            Cheeps = cheeps;
            if (User.Identity.IsAuthenticated)
            {
                var authorEmail = User.FindFirst(ClaimTypes.Name)?.Value;
                var loggedInAuthor = await _authorRepository.FindAuthorWithEmail(authorEmail);
                followedAuthors = await _authorRepository.getFollowing(loggedInAuthor.AuthorId);
            }

            return Page();
        }
        else
        {
            //Only loads the cheep that the author has written
            Author author = await _authorRepository.FindAuthorWithName(pageUser);

            //Sorts and converts the cheeps into cheepdto
            List<CheepDTO> cheeps = author.Cheeps
                .OrderByDescending(cheep => cheep.TimeStamp)
                .Select(cheep => new CheepDTO
                {
                    AuthorDTO = cheep.Author != null ? cheep.Author.Name : "Unknown",
                    Text = cheep.Text,
                    TimeStamp = cheep.TimeStamp.ToString("g")
                }).ToList();


            Cheeps = cheeps;
            if (User.Identity.IsAuthenticated)
            {
                var authorEmail = User.FindFirst(ClaimTypes.Name)?.Value;
                var loggedInAuthor = await _authorRepository.FindAuthorWithEmail(authorEmail);
                followedAuthors = await _authorRepository.getFollowing(loggedInAuthor.AuthorId);
            }
            return Page();
        }
    }
    
    public async Task<ActionResult> OnPost()
    {
        var authorName = User.FindFirst("Name")?.Value;
        
        Author author = await _authorRepository.FindAuthorWithName(authorName);
        
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
        
        Console.WriteLine("Number of followed authors" + followedAuthors.Count);

        return RedirectToPage();
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

        return RedirectToPage();
    }
    
}