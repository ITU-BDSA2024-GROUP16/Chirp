namespace Chirp;

public class Cheep
{
    public int CheepId { get; set; }
    public string Text { get; set; }
    public DateTime TimeStamp { get; set; }
    
    //Put AuthorId as foreign key form Author. DbInitializer could not handle the value unless this was put
    public int AuthorId { get; set; }
    public Author Author { get; set; }
}