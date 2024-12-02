using System.Collections.Generic; // Ensure this is included for List<T>
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Chirp.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity; // Ensure this includes Author and CheepDBContext
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;
using Assert = Xunit.Assert;

namespace Chirp.Infrastructure.Test
{
    public class IntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;
        private readonly CustomWebApplicationFactory _factory;

        public IntegrationTests(CustomWebApplicationFactory factory, ITestOutputHelper output)
        {
            _output = output;
            _factory = factory;
            //_signInManager = signInManager;

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
            HttpResponseMessage response = await _client.GetAsync("/");

            // Handle and output error response details if not successful
            if (!response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                _output.WriteLine(
                    $"Failed to access home page. Status code: {response.StatusCode}, Response content: {content}");
            }

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
                Name = "Lars McKoy11",
                Email = "McManden11@gmail.com",
                Cheeps = new List<Cheep>(),
                FollowedAuthors = new List<Author>(),
                Followers = new List<Author>()
            };

            var TestCheep = new Cheep()
            {
                CheepId = 10000010,
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