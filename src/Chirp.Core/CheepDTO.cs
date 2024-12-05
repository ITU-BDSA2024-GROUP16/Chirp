using System.ComponentModel.DataAnnotations;

namespace Chirp.Core
{
    public class CheepDTO
    {
        public string? AuthorName { get; set; } 
        [Key]
        public string? Text { get; set; } 
        public string? TimeStamp { get; set; }
        public int Likes { get; set; } = 0;
    }
}