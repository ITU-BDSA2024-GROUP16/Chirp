using System.Collections;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Core
{
    public class Author : IdentityUser
    {
    public int AuthorId { get; set; }
    public string? Name { get; set; }
    public ICollection<Cheep>? Cheeps { get; set; }
    public List<Author>? FollowedAuthors { get; set; }
    public List<Author>? Followers { get; set; }
    
    }   
}