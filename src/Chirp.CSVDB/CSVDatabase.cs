using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
namespace SimpleDB;


public class CSVDatabase<T> : IDatabaseRepository<T>
{
    
    private string _filePath;
    
    public CSVDatabase(string filePath)
    {
        _filePath = filePath;
    }
    
    public IEnumerable<T> Read(int? limit = null)
    {
        using var reader = new StreamReader(_filePath);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true, 
        });
        //Used .ToList() to be able to access the records outside of the using blocks
        return csv.GetRecords<T>().ToList();
        
    }

    public void Store(string[] args)
    {
        using var writer = new StreamWriter("../../data/chirp_cli_db.csv", append: true);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        
        var cheep = new Cheep
        {
            Author = Environment.MachineName,
            Message = string.Join(" ", args[0..]), // Combine all message arguments
            Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds()
        };
        
        
        csv.WriteRecord(cheep);
        csv.NextRecord();
    }
}


