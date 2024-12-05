using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Chirp.Core;
using Chirp.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Pages;

public class PublicModel : PageModel
{
    public readonly IAuthorRepository AuthorRepository;
    public readonly ICheepRepository CheepRepository;
    public readonly SignInManager<Author> SignInManager;
    public List<CheepDTO> Cheeps { get; set; } = new List<CheepDTO>();
    public  int PageSize = 32;
    public int PageNumber { get; set; }
    [BindProperty]
    [StringLength(160, ErrorMessage = "Cheep cannot be more than 160 characters.")]
    public string? Text { get; set; }
    public List<Author> Authors { get; set; } = new List<Author>();
    public List<Cheep> LikedCheeps { get; set; } = new List<Cheep>();
    public List<Author> FollowedAuthors { get; set; } = new List<Author>();

    public PublicModel(ICheepRepository cheepRepository, IAuthorRepository authorRepository, SignInManager<Author> signInManager)
    {
        CheepRepository = cheepRepository;
        AuthorRepository = authorRepository;
        SignInManager = signInManager;
    }

    public async Task<ActionResult> OnGet()
    {
        //check if logged-in user exists in database, otherwise log out and redirect to public timeline
        if (SignInManager.IsSignedIn(User) 
            && !string.IsNullOrEmpty(User.Identity?.Name) 
            && await AuthorRepository.FindIfAuthorExistsWithEmail(User.Identity.Name) == false)
        {
            await SignInManager.SignOutAsync();
            var baseUrl = $"{Request.Scheme}://{Request.Host}"; 
            return Redirect($"{baseUrl}/");
        }
        
        //default to page number 1 if no page is specified
        var pageQuery = Request.Query["page"];
        PageNumber = int.TryParse(pageQuery, out int page) ? page : 1;
        
        Cheeps = await CheepRepository.GetCheeps(PageNumber, PageSize);
        
        if (User.Identity?.IsAuthenticated == true)
        {
            var authorEmail = User.FindFirst(ClaimTypes.Name)?.Value;
            if (!string.IsNullOrEmpty(authorEmail))
            {
                var loggedInAuthor = await AuthorRepository.FindAuthorWithEmail(authorEmail);
                FollowedAuthors = await AuthorRepository.getFollowing(loggedInAuthor.AuthorId);
            }
        }
        return Page();
    }
    
    public async Task<ActionResult> OnPost()
    {
        var authorName = User.FindFirst(ClaimTypes.Name)?.Value;
        if (string.IsNullOrEmpty(authorName))
        {
            throw new ArgumentException("Author name cannot be null or empty.");
        }
        
        var author = await AuthorRepository.FindAuthorWithEmail(authorName);
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
        var authorName = User.FindFirst(ClaimTypes.Name)?.Value;
        if (string.IsNullOrEmpty(authorName))
        {
            throw new ArgumentException("Author name cannot be null or empty.");
        }
        
        var author = await AuthorRepository.FindAuthorWithEmail(authorName);
        
        //Finds the author that the logged in author wants to follow
        var followAuthor = await AuthorRepository.FindAuthorWithName(followAuthorName);
        
        await AuthorRepository.UnFollowUserAsync(author.AuthorId, followAuthor.AuthorId);
        
        //updates the current author's list of followed authors
        FollowedAuthors = await AuthorRepository.getFollowing(author.AuthorId);
        
        return RedirectToPage();
    }

    public async Task<ActionResult> OnPostLike(string authorDto, string text, string timeStamp, int likes)
    {
        // Find the author that's logged in
        var authorName = User.FindFirst("Name")?.Value;
        if (string.IsNullOrEmpty(authorName))
        {
            throw new ArgumentException("Author name cannot be null or empty.");
        }

        var author = await AuthorRepository.FindAuthorWithName(authorName);
        var cheep = await CheepRepository.FindCheep(text,timeStamp, authorDto);
        
        // Adds the cheep to the author's list of liked cheeps
        await CheepRepository.LikeCheep(cheep, author);
        
        LikedCheeps = await AuthorRepository.getLikedCheeps(author.AuthorId);
        
        return RedirectToPage();
    }

    
    public async Task<ActionResult> OnPostUnLike(string authorDto, string text, string timeStamp, int likes)
    {
        // Find the author that's logged in
        var authorName = User.FindFirst("Name")?.Value;
        if (string.IsNullOrEmpty(authorName))
        {
            throw new ArgumentException("Author name cannot be null or empty.");
        }

        var author = await AuthorRepository.FindAuthorWithName(authorName);
        var cheep = await CheepRepository.FindCheep(text,timeStamp,authorDto);
        
        await CheepRepository.UnLikeCheep(cheep, author);
        
        LikedCheeps = await AuthorRepository.getLikedCheeps(author.AuthorId);
        
        return RedirectToPage();
    }

    public async Task<bool> DoesUserLikeCheep(string authorDto, string text, string timeStamp)
    {
        var authorName = User.FindFirst("Name")?.Value;
        if (string.IsNullOrEmpty(authorName))
        {
            throw new ArgumentException("Author name cannot be null or empty.");
        }
        
        var author = await AuthorRepository.FindAuthorWithLikes(authorName);
        var cheep = await CheepRepository.FindCheep(text,timeStamp,authorDto);
        
        return await CheepRepository.DoesUserLikeCheep(cheep, author);
    }
}

