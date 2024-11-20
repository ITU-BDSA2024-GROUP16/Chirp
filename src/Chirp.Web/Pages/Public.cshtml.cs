﻿using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Chirp.Core;
using Chirp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Pages;

public class PublicModel : PageModel
{
    public readonly ICheepRepository _cheepRepository;
    public List<CheepDTO> Cheeps { get; set; } = new List<CheepDTO>();
    private const int PageSize = 32;
    public int PageNumber { get; set; }
    [BindProperty]
    [StringLength(160, ErrorMessage = "Cheep cannot be more than 160 characters.")]
    public string Text { get; set; }
    public List<Author> Authors { get; set; } = new List<Author>();
    public List<Author> followedAuthors { get; set; } = new List<Author>();

    public PublicModel(ICheepRepository cheepRepository)
    {
        _cheepRepository = cheepRepository;
    }

    public async Task<ActionResult> OnPostFollow(string followAuthorName)
    {
        //Finds the logged in user
        var authorName = User.FindFirst(ClaimTypes.Name)?.Value;
        var author = await _cheepRepository.FindAuthorWithEmail(authorName);
        
        //Finds the user that the current user wants to follow
        var author2 = await _cheepRepository.FindAuthorWithEmail(followAuthorName);
       
        followedAuthors = await _cheepRepository.getFollowing(author.AuthorId);
        _cheepRepository.FollowUserAsync(author.AuthorId, author2.AuthorId);
        
        Console.WriteLine("Number of followed authors" + followedAuthors.Count);

        return Page();
    }
    
    public async Task<ActionResult> OnPostUnfollow(string followAuthorName)
    {
        //Finds the logged in user
        var authorName = User.FindFirst(ClaimTypes.Name)?.Value;
        var author = await _cheepRepository.FindAuthorWithEmail(authorName);
        
        //Finds the user that the current user wants to follow
        var author2 = await _cheepRepository.FindAuthorWithEmail(followAuthorName);
       
        followedAuthors = await _cheepRepository.getFollowing(author.AuthorId);
        _cheepRepository.UnFollowUserAsync(author.AuthorId, author2.AuthorId);
        
        Console.WriteLine("Number of followed authors" + followedAuthors.Count);

        return Page();
    }
    

    public async Task<ActionResult> OnGet()
    {
        //default to page number 1 if no page is specified
        var pageQuery = Request.Query["page"];
        PageNumber = int.TryParse(pageQuery, out int page) ? page : 1;
        
        Cheeps = await _cheepRepository.GetCheeps(PageNumber, PageSize);
        return Page();
    }
    
    public async Task<ActionResult> OnPost()
    {
        var authorName = User.FindFirst(ClaimTypes.Name)?.Value;
        
        var author = await _cheepRepository.FindAuthorWithEmail(authorName);
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

