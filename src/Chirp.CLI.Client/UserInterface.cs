﻿namespace Chirp.CLI;

using SimpleDB;

public class UserInterface
{
    public static void printCheeps(IEnumerable<Cheep> cheeps)
    {
        foreach (var cheep in cheeps)
        {
            DateTimeOffset adjustedDateTime = DateTimeOffset.FromUnixTimeSeconds(cheep.Timestamp).AddHours(2);
            
            Console.WriteLine($"{cheep.Author} @ {adjustedDateTime:MM/dd/yy HH:mm:ss}: {cheep.Message}");
        }
    }

    public static void printCheeps(Cheep cheep)
    {
        DateTimeOffset adjustedDateTime = DateTimeOffset.FromUnixTimeSeconds(cheep.Timestamp).AddHours(2);
            
        Console.WriteLine($"{cheep.Author} @ {adjustedDateTime:MM/dd/yy HH:mm:ss}: {cheep.Message}");
    }
}