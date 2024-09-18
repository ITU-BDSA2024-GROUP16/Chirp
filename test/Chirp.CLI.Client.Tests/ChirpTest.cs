using System;
using Xunit;
using SimpleDB;
using Chirp.CLI;

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
    }
}