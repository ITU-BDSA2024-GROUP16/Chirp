// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Chirp.Core;
using Chirp.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Chirp.Web.Areas.Identity.Pages.Account.Manage
{
    public class DownloadPersonalDataModel : PageModel
    {
        private readonly UserManager<Author> _userManager;
        private readonly ILogger<DownloadPersonalDataModel> _logger;
        private readonly IAuthorRepository _authorRepository;


        public DownloadPersonalDataModel(
            UserManager<Author> userManager,
            ILogger<DownloadPersonalDataModel> logger,
            IAuthorRepository authorRepository)
        {
            _userManager = userManager;
            _logger = logger;
            _authorRepository = authorRepository;

        }

        public IActionResult OnGet()
        {
            return NotFound();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var authorName = User.FindFirst("Name")?.Value ?? "User";                                                                                   
                                                                                                                                            
            var author = await _authorRepository.FindAuthorWithName(authorName);                                                                        
                                                                                                                                            
            // makes a list of links, of users from the FollowedAuthors list                                                                            
            var followedAuthorsLinks = author.FollowedAuthors?.Select(followedAuthor =>                                                                 
                $"http://localhost:5273/{{Uri.EscapeDataString(followedAuthor.UserName)}}").ToList() ?? new List<string>();                             
                                                                                                                                            
            var data = new                                                                                                                              
            {                                                                                                                                           
                Name = author.Name,                                                                                                                     
                Email = author.Email,                                                                                                                   
                Phonenumber = author.PhoneNumber,                                                                                                       
                FollowedAuthors = followedAuthorsLinks,                                                                                                 
                Cheeps = author.Cheeps                                                                                                                  
            };                                                                                                                                          
                                                                                                                                                        
            // Serializering ata to JSON                                                                                                                
            var jsonData = JsonSerializer.Serialize(data);                                                                                              
                                                                                                                                                        
            // Converting JSON to byte-array                                                                                                            
            var bytes = Encoding.UTF8.GetBytes(jsonData);                                                                                               
                                                                                                                                                        
            // Returns the complete file to the user                                                                                                    
            return File(bytes, "application/json", "PersonalData.json");                                                                                
                                                                                                                                                        
        }
    }
}
