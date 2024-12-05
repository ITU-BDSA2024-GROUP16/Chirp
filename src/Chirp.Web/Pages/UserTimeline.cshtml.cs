using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Chirp.Core;
using Chirp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace Chirp.Web.Pages;

public class UserTimelineModel : PageModel
{
    public readonly IAuthorRepository AuthorRepository;
    public readonly ICheepRepository CheepRepository;
    public List<CheepDTO> Cheeps { get; set; } = new List<CheepDTO>();
    public int PageSize = 32;
    public int PageNumber { get; set; } = 1;
    [BindProperty]
    [StringLength(160, ErrorMessage = "Cheep cannot be more than 160 characters.")]
    public string? Text { get; set; }
    public List<Author> FollowedAuthors { get; set; } = new List<Author>();



    public UserTimelineModel(ICheepRepository cheepRepository, IAuthorRepository authorRepository)
    {
        CheepRepository = cheepRepository;
        AuthorRepository = authorRepository;
    }

    public async Task<ActionResult> OnGet()
    {
        //Gets the authorName from the currently LOGGED IN user
        var authorName = User.FindFirst("Name")?.Value ?? "User";
        //Gets the author name from the URL.
        var pageUser = HttpContext.GetRouteValue("author")?.ToString() ?? "DefaultUser";


        // This checks if the logged in user's USERNAME equals to the value from the UserTimeline URL
        if (authorName == pageUser)
        {
            var pageQuery = Request.Query["page"];
            if (!string.IsNullOrEmpty(pageQuery))
            {
                PageNumber = int.TryParse(pageQuery.ToString(), out int page) ? page : 1;
            }
            else
            {
                PageNumber = 1;
            }

            
            //Loads the author with their cheeps and followers using the authors name
            Author author = await AuthorRepository.FindAuthorWithName(authorName);

            //Creates a list to gather the author and all its followers
            var allAuthors = new List<Author> { author };
            
            //Adds all the followers to the list
            allAuthors.AddRange(author.FollowedAuthors ?? Enumerable.Empty<Author>());
            
            // Ensure PageNumber is valid and greater than 0
            PageNumber = Math.Max(1, PageNumber); // This ensures PageNumber is never less than 1

            // Sorts and converts the cheeps into cheepdto
            List<CheepDTO> cheeps = allAuthors
                .SelectMany(a => a.Cheeps ?? Enumerable.Empty<Cheep>())  
                .OrderByDescending(cheep => cheep.TimeStamp)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .Select(cheep => new CheepDTO
                {
                    AuthorName = cheep.Author != null ? cheep.Author.Name : "Unknown",
                    Text = cheep.Text,
                    TimeStamp = cheep.TimeStamp.ToString("g")
                })
                .ToList();

            // Assign the combined list to Cheeps
            Cheeps = cheeps;
            if (User.Identity?.IsAuthenticated == true)
            {
                var authorEmail = User.FindFirst(ClaimTypes.Name)?.Value;

                // Check if authorEmail is null or empty
                if (string.IsNullOrEmpty(authorEmail))
                {
                    // Throw an exception if the email is missing
                    throw new InvalidOperationException("User's email is missing or not authenticated.");
                }

                // Proceed with the method call if the email is valid
                var loggedInAuthor = await AuthorRepository.FindAuthorWithEmail(authorEmail);
                FollowedAuthors = await AuthorRepository.getFollowing(loggedInAuthor.AuthorId);
            }

            return Page();
        }
        else
        {
            //Only loads the cheep that the author has written
            Author author = await AuthorRepository.FindAuthorWithName(pageUser);

            List<CheepDTO> cheeps = author.Cheeps?
                .OrderByDescending(cheep => cheep.TimeStamp)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .Select(cheep => new CheepDTO
                {
                    AuthorName = cheep.Author != null ? cheep.Author.Name : "Unknown",
                    Text = cheep.Text,
                    TimeStamp = cheep.TimeStamp.ToString("g")
                })
                .ToList() ?? new List<CheepDTO>(); // If Cheeps is null, use an empty list

            
            Cheeps = cheeps;
            if (User.Identity?.IsAuthenticated == true)
            {
                var authorEmail = User.FindFirst(ClaimTypes.Name)?.Value;

                // Check if authorEmail is null or empty
                if (string.IsNullOrEmpty(authorEmail))
                {
                    // Throw an exception if the email is missing
                    throw new InvalidOperationException("User's email is missing or not authenticated.");
                }

                // Proceed with the method call if the email is valid
                var loggedInAuthor = await AuthorRepository.FindAuthorWithEmail(authorEmail);
                FollowedAuthors = await AuthorRepository.getFollowing(loggedInAuthor.AuthorId);
            }
            return Page();
        }
    }
    
    public async Task<ActionResult> OnPost()
    {
        var authorName = User.FindFirst("Name")?.Value;
        
        if (string.IsNullOrEmpty(authorName))
        {
            throw new ArgumentException("Author name cannot be null or empty.");
        }

        Author author = await AuthorRepository.FindAuthorWithName(authorName);

        
        var cheep = new Cheep
        {
            AuthorId = author.AuthorId,
            Text = Text,
            TimeStamp = DateTime.Now,
            Author = author
        };
        
        await CheepRepository.SaveCheep(cheep, author);
        
        return RedirectToPage();
    }
    
    public async Task<ActionResult> OnPostFollow(string followAuthorName)
    {
        //Finds the author thats logged in
        var authorName = User.FindFirst(ClaimTypes.Name)?.Value;
        if (string.IsNullOrEmpty(authorName))
        {
            throw new ArgumentException("Author name cannot be null or empty.");
        }
        
        var author = await AuthorRepository.FindAuthorWithEmail(authorName);
        
        //Finds the author that the logged in author wants to follow
        var followAuthor = await AuthorRepository.FindAuthorWithName(followAuthorName);
        
        await AuthorRepository.FollowUserAsync(author.AuthorId, followAuthor.AuthorId);
        
        //updates the current author's list of followed authors
        FollowedAuthors = await AuthorRepository.getFollowing(author.AuthorId);
        
        return RedirectToPage();
    }

    public async Task<ActionResult> OnPostUnfollow(string followAuthorName)
    {
        //Finds the author thats logged in
        var authorName = User.FindFirst("Name")?.Value;
        if (string.IsNullOrEmpty(authorName))
        {
            throw new ArgumentException("Author name cannot be null or empty.");
        }
        var author = await AuthorRepository.FindAuthorWithName(authorName);
        
        //Finds the author that the logged in author wants to follow
        var followAuthor = await AuthorRepository.FindAuthorWithName(followAuthorName);
        
        await AuthorRepository.UnFollowUserAsync(author.AuthorId, followAuthor.AuthorId);
        
        //updates the current author's list of followed authors
        FollowedAuthors = await AuthorRepository.getFollowing(author.AuthorId);
        
        return RedirectToPage();
    }
}