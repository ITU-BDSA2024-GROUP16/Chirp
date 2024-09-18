using System;
using Xunit;
using SimpleDB;

namespace Chirp.Tests
{
    public class CheepTests
    {
        [Fact]
        public void ConvertFromUnixTimestamp_ShouldReturnCorrectDateTime()
        {
            // Arrange
            long unixTimestamp = 1694188800; // Example timestamp (09/08/2023 @ 12:00am UTC)
            var expectedDate = new DateTime(2023, 9, 8, 0, 0, 0, DateTimeKind.Utc);

            // Act
            var actualDate = Cheep.ConvertFromUnixTimestamp(unixTimestamp);

            // Assert
            Assert.Equal(expectedDate, actualDate);
        }
    }
}