using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Core
{
    public class Author : IdentityUser<int>
    {
    public int AuthorId { get; set; }
    public string? Name { get; set; } 
    [NotMapped] 
    public ICollection<Cheep>? Cheeps { get; set; } = new List<Cheep>();
    public List<Author>? FollowedAuthors { get; set; } = new List<Author>();
    public List<Author>? Followers { get; set; } = new List<Author>();
    public List<Cheep>? LikedCheeps { get; set; } = new List<Cheep>();
    }
}