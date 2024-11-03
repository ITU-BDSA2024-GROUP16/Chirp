using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Xunit.Abstractions;
using Assert = Xunit.Assert;

namespace Chirp.Infrastructure.Test
{
    public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;

        public IntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
        {
            _output = output; // Assigning the output to the private field

            
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
                
            {
                AllowAutoRedirect = true,
                HandleCookies = true
            });
        }

        [Fact]
        public async Task CanAccessHomePage()
        {
            // Act
            var response = await _client.GetAsync("/");
            
            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task FindTimelineByAuthor()
        {
            HttpResponseMessage response = await _client.GetAsync($"/Jacqualine Gilcoine");
            response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync();
            
            _output.WriteLine("content: {0}", content);

            Assert.Contains("Chirp!", content);
            Assert.Contains("Jacqualine", content);
        }
    }
}
