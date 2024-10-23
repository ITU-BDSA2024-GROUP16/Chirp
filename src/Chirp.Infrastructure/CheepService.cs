using Chirp.Core;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure;

public interface ICheepService {}

public class CheepService : ICheepService
{
    private readonly DBFacade _dbFacade;
    private readonly CheepDBContext _dbContext; 
    public CheepService(DBFacade dbFacade, CheepDBContext dbContext) 
    { 
        _dbFacade = dbFacade;
        _dbContext = dbContext; 
        SQLitePCL.Batteries.Init();
    }
}