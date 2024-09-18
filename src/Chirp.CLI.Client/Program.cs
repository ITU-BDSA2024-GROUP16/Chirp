using SimpleDB;
using System.CommandLine;
using Chirp.CLI;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var db = CSVDatabase<Cheep>.Instance("data\\chirp_cli_db.csv");
        
        var rootCommand = new RootCommand("This application performs 'read' and 'write' operations.");
        
        var readCommand = new Command("--read", "Reads data from the database");
        readCommand.SetHandler(() => ReadCheeps(db));
        
        var dataArgument = new Argument<string>("message", "The message to be written");
        var writeCommand = new Command("--write", "Writes data to the database")
        {
            dataArgument 
        };
        
        
        
        writeCommand.SetHandler((string data) => CreateCheepFromCommandLine(db, data), dataArgument);
        
        rootCommand.AddCommand(readCommand);
        rootCommand.AddCommand(writeCommand);
        
        return await rootCommand.InvokeAsync(args);
    }

    static void ReadCheeps(CSVDatabase<Cheep> db)
    {
        var cheeps = db.Read();
        UserInterface.printCheeps(cheeps);
    }
    static void CreateCheepFromCommandLine(CSVDatabase<Cheep> db, string data)
    {
        db.Store(new[] { data });
        Console.WriteLine($"Data '{data}' has been written to the database.");
    }
}
