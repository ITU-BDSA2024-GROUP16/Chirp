namespace SimpleDB;

public class Cheep
{
    public string Author { get; set; }
    public string Message { get; set; }
    public long Timestamp { get; set; }
    
    
        // Helper method to convert UNIX timestamp to readable time
        public static DateTime ConvertFromUnixTimestamp(long timestamp)
        {
            return DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;
        }
    
    
}
