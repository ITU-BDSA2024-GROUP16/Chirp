using System;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Please provide 'read' or 'write' command.");
            return;
        }
        
        if (args[0] == "read")
        {
            Read();
        }
        else if (args[0] == "write")
        {
            Write(args);
        }
        else
        {
            Console.WriteLine("Invalid command. Use 'read' or 'write'.");
        }
    }

    static void Read()
    {
        using var reader = new StreamReader("C:\\Users\\jacqu\\Chirp.CLI\\chirp_cli_db.csv");
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true, 
        });
        
        foreach (var cheep in csv.GetRecords<Cheep>())
        {
            DateTimeOffset adjustedDateTime = DateTimeOffset.FromUnixTimeSeconds(cheep.Timestamp).AddHours(2);
            
            Console.WriteLine($"{cheep.Author} @ {adjustedDateTime:MM/dd/yy HH:mm:ss}: {cheep.Message}");
        }
    }

    static void Write(string[] args)
    {
        using var writer = new StreamWriter("C:\\Users\\jacqu\\Chirp.CLI\\chirp_cli_db.csv", append: true);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        
        var cheep = new Cheep
        {
            Author = Environment.MachineName,
            Message = string.Join(" ", args[1..]), // Combine all message arguments
            Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds()
        };
        
        csv.WriteRecord(cheep);
        csv.NextRecord();
    }
}
