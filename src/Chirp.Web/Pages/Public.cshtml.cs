﻿using System.Security.Claims;
using Chirp.Core;
using Chirp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Pages;

public class PublicModel : PageModel
{
    private readonly ICheepRepository _cheepRepository;
    public List<CheepDTO> Cheeps { get; set; } = new List<CheepDTO>();
    private const int PageSize = 32;
    public int PageNumber { get; set; }

    public PublicModel(ICheepRepository cheepRepository)
    {
        _cheepRepository = cheepRepository;
    }

    public async Task<ActionResult> OnGet()
    {
        //default to page number 1 if no page is specified
        var pageQuery = Request.Query["page"];
        PageNumber = int.TryParse(pageQuery, out int page) ? page : 1;
        
        Cheeps = await _cheepRepository.GetCheeps(PageNumber, PageSize);
        return Page();
    }
    
    public async Task<ActionResult> OnPost(string message)
    {
        // 
        var authorName = User.FindFirst(ClaimTypes.Name)?.Value;

        if (string.IsNullOrEmpty(authorName))
        {
            ModelState.AddModelError(string.Empty, "You must be logged in to post a cheep.");
            return Page();
        }

        var author = await _cheepRepository.FindAuthorWithEmail(authorName);

        if (author == null)
        {
            ModelState.AddModelError(string.Empty, "Author not found.");
            return Page();
        }

        var newCheep = new Cheep
        {
            AuthorId = author.AuthorId,
            Text = message,
            TimeStamp = DateTime.Now,
            Author = author
        };

        await _cheepRepository.SaveCheep(newCheep);

        return RedirectToPage();
    }
}

