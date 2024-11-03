using System.Collections.Generic; // Ensure this is included for List<T>
using System.Net.Http;
using System.Threading.Tasks;
using Chirp.Core; // Ensure this includes Author and CheepDBContext
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;
using Assert = Xunit.Assert;

namespace Chirp.Infrastructure.Test
{
    public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;
        private readonly WebApplicationFactory<Program> _factory;

        public IntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
        {
            _output = output;
            _factory = factory;

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

        [Fact]
        public async Task CanCreateUserAndFindUser()
        {
            using var scope = _factory.Services.CreateScope();
            var services = scope.ServiceProvider;
            var dbContext = services.GetRequiredService<CheepDBContext>();

            
            
            var testAuthor1 = new Author
            {
                Name = "Lars McKoy",
                Email = "McManden@gmail.com",
                Cheeps = new List<Cheep>()
            };

            var TestCheep = new Cheep()
            {
                CheepId = 100000,
                Text = "Lorem ipsum dolor sit amet",
                TimeStamp = DateTime.Now,
                Author = testAuthor1,
                AuthorId = 1
            };
            
            testAuthor1.Cheeps.Add(TestCheep);
            

            // Add the new author to the database
            await dbContext.Authors.AddAsync(testAuthor1);
            await dbContext.SaveChangesAsync();

            // Perform an HTTP request to check if the new author can be found
            HttpResponseMessage response = await _client.GetAsync($"/{testAuthor1.Name}");
            response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync();

            _output.WriteLine("content: {0}", content); // Log the content for debugging

            // Assert that the response contains expected values
            Assert.Contains("Chirp!", content);
            Assert.Contains("Lars", content);
        }
    }
}
