using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Chirp;

public class DBFacade
{
    private readonly CheepDBContext _context;
    
    //Dependency injection of CheepDBContext inside of DBFacade
    public DBFacade(CheepDBContext context)
    {
        _context = context;
    }
    

    public List<Cheep> LoadCheeps(int pageNumber)
    {
        int pageSize = 32;
        var cheeps = _context.Cheeps
            .Include(c => c.Author)
            .OrderByDescending(c => c.TimeStamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return cheeps;
    }
}