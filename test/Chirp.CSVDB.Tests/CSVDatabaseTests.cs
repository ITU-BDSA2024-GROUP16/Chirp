using System;
using System.Collections.Generic;
using System.IO;
using SimpleDB;
using Xunit;

namespace Chirp.CSVDB.Tests;

public class CSVDatabaseTests
{
    private const string TestFilePath = "test_database.csv";

    public CSVDatabaseTests()
    {
        if (File.Exists(TestFilePath))
        {
            File.Delete(TestFilePath);
        }
    }

    [Fact]
    public void StoreAndRead_Record_ShouldStoreAndRetrieveRecord()
    {
        var database = CSVDatabase<TestRecord>.Instance(TestFilePath);
        var expectedRecord = new TestRecord
        {
            Id = 1,
            Name = "John Doe",
            Age = 30
        };
        
        database.Store(expectedRecord); 
        var records = database.Read(); 
        
        Assert.NotEmpty(records); 
        var retrievedRecord = Assert.Single(records); 
        Assert.Equal(expectedRecord.Id, retrievedRecord.Id); 
        Assert.Equal(expectedRecord.Name, retrievedRecord.Name); 
        Assert.Equal(expectedRecord.Age, retrievedRecord.Age); 
    }

    
public class TestRecord
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
}
}
