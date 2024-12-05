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
    public readonly IAuthorRepository _authorRepository;
    public readonly ICheepRepository _cheepRepository;
    public readonly SignInManager<Author> _signInManager;
    public List<CheepDTO> Cheeps { get; set; } = new List<CheepDTO>();
    private const int PageSize = 32;
    public int PageNumber { get; set; }
    [BindProperty]
    [StringLength(160, ErrorMessage = "Cheep cannot be more than 160 characters.")]
    public string? Text { get; set; }
    public List<Author> Authors { get; set; } = new List<Author>();
    public List<Author> followedAuthors { get; set; } = new List<Author>();
    public List<Cheep> likedCheeps { get; set; } = new List<Cheep>();

    public PublicModel(ICheepRepository cheepRepository, IAuthorRepository authorRepository, SignInManager<Author> signInManager)
    {
        _cheepRepository = cheepRepository;
        _authorRepository = authorRepository;
        _signInManager = signInManager;
    }

    public async Task<ActionResult> OnGet()
    {
        //check if logged-in user exists in database, otherwise log out and redirect to public timeline
        if (_signInManager.IsSignedIn(User) 
            && !string.IsNullOrEmpty(User.Identity?.Name) 
            && await _authorRepository.FindIfAuthorExistsWithEmail(User.Identity.Name) == false)
        {
            await _signInManager.SignOutAsync();
            return Redirect("http://localhost:5273/");
        }
        
        //default to page number 1 if no page is specified
        var pageQuery = Request.Query["page"];
        PageNumber = int.TryParse(pageQuery, out int page) ? page : 1;
        
        Cheeps = await _cheepRepository.GetCheeps(PageNumber, PageSize);
        
        if (User.Identity?.IsAuthenticated == true)
        {
            var authorEmail = User.FindFirst(ClaimTypes.Name)?.Value;
            if (!string.IsNullOrEmpty(authorEmail))
            {
                var loggedInAuthor = await _authorRepository.FindAuthorWithEmail(authorEmail);
                followedAuthors = await _authorRepository.getFollowing(loggedInAuthor.AuthorId);
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
        
        var author = await _authorRepository.FindAuthorWithEmail(authorName);
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
        if (string.IsNullOrEmpty(authorName))
        {
            throw new ArgumentException("Author name cannot be null or empty.");
        }
        
        var author = await _authorRepository.FindAuthorWithEmail(authorName);
        
        //Finds the author that the logged in author wants to follow
        var followAuthor = await _authorRepository.FindAuthorWithName(followAuthorName);
        
        await _authorRepository.FollowUserAsync(author.AuthorId, followAuthor.AuthorId);
        
        //updates the current author's list of followed authors
        followedAuthors = await _authorRepository.getFollowing(author.AuthorId);
        
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
        
        var author = await _authorRepository.FindAuthorWithEmail(authorName);
        
        //Finds the author that the logged in author wants to follow
        var followAuthor = await _authorRepository.FindAuthorWithName(followAuthorName);
        
        await _authorRepository.UnFollowUserAsync(author.AuthorId, followAuthor.AuthorId);
        
        //updates the current author's list of followed authors
        followedAuthors = await _authorRepository.getFollowing(author.AuthorId);
        
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

        var author = await _authorRepository.FindAuthorWithName(authorName);
        var cheep = await _cheepRepository.FindCheep(text,timeStamp, authorDto);
        
        // Adds the cheep to the author's list of liked cheeps
        await _cheepRepository.LikeCheep(cheep, author);
        
        likedCheeps = await _authorRepository.getLikedCheeps(author.AuthorId);
        
        return RedirectToPage();
    }

    
    public async Task<ActionResult> OnPostUnLike(string authorDto, string text, string timeStamp, int? likes)
    {
        // Find the author that's logged in
        var authorName = User.FindFirst("Name")?.Value;
        if (string.IsNullOrEmpty(authorName))
        {
            throw new ArgumentException("Author name cannot be null or empty.");
        }

        var author = await _authorRepository.FindAuthorWithName(authorName);
        var cheep = await _cheepRepository.FindCheep(text,timeStamp,authorDto);
        
        await _cheepRepository.UnLikeCheep(cheep, author);
        
        likedCheeps = await _authorRepository.getLikedCheeps(author.AuthorId);
        
        return RedirectToPage();
    }

    public async Task<bool> DoesUserLikeCheep(string authorDto, string text, string timeStamp)
    {
        var authorName = User.FindFirst("Name")?.Value;
        if (string.IsNullOrEmpty(authorName))
        {
            throw new ArgumentException("Author name cannot be null or empty.");
        }
        
        var author = await _authorRepository.FindAuthorWithLikes(authorName);
        var cheep = await _cheepRepository.FindCheep(text,timeStamp,authorDto);
        
        return await  _cheepRepository.DoesUserLikeCheep(cheep, author);
    }
}

