
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
namespace SimpleDB;


public class CSVDatabase<T> : IDatabaseRepository<T>
{
    
    
    private string _filePath;
    private static CSVDatabase<T> instance = null;
    private static readonly object padlock = new object();
    
    
    public CSVDatabase(string filePath)
    {
        _filePath = filePath;
    }
    
    
    public static CSVDatabase<T> Instance(string filePath)
    {
        lock (padlock)
        {
            if (instance == null)
            {
                instance = new CSVDatabase<T>(filePath);
            }
            return instance;
        }
    }
    
    
    
    public IEnumerable<T> Read()
    {
        using var reader = new StreamReader(_filePath);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true, 
        });
        //Used .ToList() to be able to access the records outside of the using blocks
        return csv.GetRecords<T>().ToList();
        
    }

    public void Store(T record)
    {
        using var writer = new StreamWriter(_filePath, append: true);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecord(record);
        csv.NextRecord();
    }
}


