using System;
using System.Globalization;
using Xunit;
using SimpleDB;
using Chirp.CLI;
using CsvHelper;
using CsvHelper.Configuration;
using Moq;

namespace Chirp.Tests
{
    public class CheepTests
    {
        [Fact]
        public void ConvertFromUnixTimestamp_ShouldReturnCorrectDateTime()
        {
            // Arrange
            long unixTimestamp = 1694188800; 
            var expectedDate = new DateTime(2023, 9, 8, 16, 0, 0, DateTimeKind.Utc);

            // Act
            var actualDate = Cheep.ConvertFromUnixTimestamp(unixTimestamp);

            // Assert
            Assert.Equal(expectedDate.ToUniversalTime(), actualDate.ToUniversalTime());
        }
        
        [Fact]
        public void Instance_ShouldReturnSameInstance_WhenCalledMultipleTimes()
        {
            // Arrange
            var db1 = CSVDatabase<string>.Instance("testfile.csv");
            var db2 = CSVDatabase<string>.Instance("anotherfile.csv");

            // Act & Assert
            Assert.Same(db1, db2);  // Both instances should be the same
        }
        
    }
}