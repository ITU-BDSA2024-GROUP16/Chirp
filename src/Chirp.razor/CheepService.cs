using Chirp;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
/*
public record CheepViewModel(string Author, string Message, string Timestamp);

public interface ICheepService
{
    Task<List<CheepViewModel>> GetCheeps(int pageNumber, int pageSize);
    Task<List<CheepViewModel>> GetCheepsFromAuthor(string author, int pageNumber, int pageSize);
}

public class CheepService : ICheepService
{
    public CheepService()
    {
        // Initialize SQLite
        SQLitePCL.Batteries.Init();
    }
    
   
    /*
    public List<CheepViewModel> GetCheeps(int pageNumber, int pageSize)
    {
        return DBFacade.LoadCheeps(pageNumber)
            .ToList();
    }
    */
/*
    public async Task<List<CheepViewModel>> GetCheeps(int pageNumber, int pageSize)
    {
        var cheeps = await DBFacade.LoadCheeps(pageNumber);
        return cheeps.ToList();
    }
    
    public async Task<List<CheepViewModel>> GetCheepsFromAuthor(string author, int pageNumber, int pageSize)
    {
        var cheeps = await DBFacade.LoadCheeps(pageNumber);
        return cheeps.Where(x => x.Author == author).ToList();
    }
    */

/*
    public List<CheepViewModel> GetCheepsFromAuthor(string author, int pageNumber, int pageSize)
    {
        // filter by the provided author name
        return DBFacade.LoadCheeps(pageNumber)
            .Where(x => x.Author == author)
            .ToList();
    }
    
}
*/
