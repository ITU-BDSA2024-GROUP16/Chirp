using System;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using SimpleDB;
using System.CommandLine;
using Chirp.CLI;

class Program
{
    static void Main(string[] args)
    {
        // I imported the Database where the cheeps are stored. (Insert your own filepath)
        var db = new CSVDatabase<Cheep>("C:\\Users\\jacqu\\Chirp.CLI\\chirp_cli_db.csv");
        
        if (args.Length == 0)
        {
            Console.WriteLine("Please provide 'read' or 'write' command.");
            return;
        }
        
        if (args[0] == "read")
        {
            Read(db);
        }
        else if (args[0] == "write")
        {
            Write(db,args);
        }
        else
        {
            Console.WriteLine("Invalid command. Use 'read' or 'write'.");
        }
    }

    static void Read(CSVDatabase<Cheep> db)
    {
        // Gets IEnumerable<Cheep> from CSVDatabase
        var cheeps = db.Read();
        
        // Calls on UserInterface to print the cheeps from CSVDatabase
        UserInterface.printCheeps(cheeps);
        
    }

    //Would it make sense to remove this entire method, and just use db.Store() in the args section?
    static void Write(CSVDatabase<Cheep> db,string[] args)
    {
        db.Store(args);
    }
}
