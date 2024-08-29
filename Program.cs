using System.Text.RegularExpressions;
using System;

// Vælg om du vil læse eksisterende cheeps eller vil skrive en ny
// Skriv enten dotnet run -- read ELLEr dotnet run -- write <Din Text>

if(args[0] == "read") {
    using StreamReader reader = new StreamReader("C:\\Users\\jacqu\\Chirp.CLI\\chirp_cli_db.csv");
    {
        string line = reader.ReadLine();
        while ((line = reader.ReadLine()) != null)
        {
            // Vores regex som indeler i 3 grupper. "Author", "Message", "Timestamp"
            string pattern = "^(\\w+),\"([^\"]+)\",(\\d+)$";

            // Regex matcher hver "Line" i vores input".
            Match match = Regex.Match(line, pattern);

            // Input bliver matchet i respektive grupper.
            string author = match.Groups[1].Value;
            string message = match.Groups[2].Value;
            string timestamp = match.Groups[3].Value;

            // Timestamp, som er i unix-seconds, bliver convertet til dato og tid (+ med 2 timer if.t tidsforksel)
            DateTimeOffset adjustedDateTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(timestamp)).AddHours(2);

            //Print den samlede cheep (Formatet på dato kan justeres. Eksemplet var med NA datering, men kan hurtig ændres)
            Console.WriteLine(author + " @ " + adjustedDateTime.ToString("MM/dd/yy HH:mm:ss") + ": " + message);
        }
    } 

} else if (args[0] == "write") {
    using (StreamWriter sw = File.AppendText("C:\\Users\\jacqu\\Chirp.CLI\\chirp_cli_db.csv"))
    {
        sw.WriteLine("");
        // Skriver din enheds navn i filen
        sw.Write(Environment.MachineName + ",\"");
        
        for (int i = 1; i < args.Length; i++)
        {
            //Hvert ord i args bliver skrevet ind i filen
            sw.Write(args[i]);
            sw.Write(" ");
        }
        // Skriver tiden netop nu
        sw.Write( "\"," + ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds());
    }
}